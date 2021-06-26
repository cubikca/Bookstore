using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.ObjectFillers;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NUnit.Framework;

namespace Bookstore.Services.Book.Tests
{
    public class BookTests
    {
        private IServiceProvider _services;
        private IRequestClient<SaveBookCommand> _saveBookCommand;
        private IRequestClient<FindBooksQuery> _findBooksQuery;
        private IRequestClient<RemoveBookCommand> _removeBookCommand;
        private BookFiller _bookFiller;
        
        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var azureConfig = config.GetSection("Azure");
            var certificate = new X509Certificate2(azureConfig["CertificatePath"], azureConfig["CertificatePassphrase"]);
            var credential = new ClientCertificateCredential(azureConfig["TenantId"], azureConfig["ApplicationId"], certificate);
            services.AddSingleton(credential);
            var keyVaultConfig = config.GetSection("KeyVault");
            var secretClient = new SecretClient(new Uri(keyVaultConfig["Url"]), credential, new SecretClientOptions());
            services.AddSingleton(secretClient);
            var storageConfig = config.GetSection("AzureStorage");
            var blobServiceClient =
                new BlobServiceClient(new Uri($"https://{storageConfig["AccountName"]}.blob.core.windows.net"),
                    new ManagedIdentityCredential());
            var messageDataRepository = blobServiceClient.CreateMessageDataRepository(storageConfig["MessageDataContainer"]);
            services.AddSingleton<IMessageDataRepository>(messageDataRepository);
            services.AddLogging(log => log.AddConsole());
            services.AddMassTransit(mt =>
            {
                mt.AddRequestClient<SaveBookCommand>();
                mt.AddRequestClient<FindBooksQuery>();
                mt.AddRequestClient<RemoveBookCommand>();
                mt.UsingAzureServiceBus((_, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var booksConfig = config.GetSection("BooksService");
                    var booksConnection = $"sb://{booksConfig["ServiceBusNamespace"]}.servicebus.windows.net";
                    var secretName = booksConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(booksConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(booksConfig["AccessKeyName"],
                            sharedAccessKey)
                    };
                    sb.Host(hostSettings);
                    sb.UseJsonSerializer();
                });
            });
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build();
            var services = new ServiceCollection();
            ConfigureServices(services, config);
            _services = services.BuildServiceProvider();
            _saveBookCommand = _services.GetRequiredService<IRequestClient<SaveBookCommand>>();
            _findBooksQuery = _services.GetRequiredService<IRequestClient<FindBooksQuery>>();
            _removeBookCommand = _services.GetRequiredService<IRequestClient<RemoveBookCommand>>();
            _bookFiller = new BookFiller();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var book = _bookFiller.FillBook();
            var saveResponse = await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand { Book = book });
            Assert.IsTrue(saveResponse.Message.Success); 
            Assert.NotNull(saveResponse.Message.Book);
            Assert.AreEqual(book.Id, saveResponse.Message.Book.Id);
            Assert.AreEqual(book, saveResponse.Message.Book);
            book = _bookFiller.FillBook();
            book.Id = saveResponse.Message.Book.Id;
            var updateResponse = await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand { Book = book });
            Assert.IsTrue(updateResponse.Message.Success);
            Assert.NotNull(updateResponse.Message.Book);
            Assert.AreEqual(book.Id, updateResponse.Message.Book.Id);
            Assert.AreEqual(book, updateResponse.Message.Book);
        }

        [Test]
        public async Task TestFind()
        {
            var book = _bookFiller.FillBook();
            await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand { Book = book });
            var findResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery { BookId = book.Id });
            var findAllResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery());
            Domains.Book.Models.Book found = null;
            var allBooks = Enumerable.Empty<Domains.Book.Models.Book>().ToList();
            if (findResponse.Message.Results.HasValue)
            {
                var json = await findResponse.Message.Results.Value;
                var books = JsonConvert.DeserializeObject<List<Domains.Book.Models.Book>>(json);
                found = books?.SingleOrDefault();
            }
            Assert.NotNull(found);
            Assert.AreEqual(book, found);
            if (findAllResponse.Message.Results.HasValue)
            {
                var json = await findAllResponse.Message.Results.Value;
                allBooks = JsonConvert.DeserializeObject<List<Domains.Book.Models.Book>>(json) ?? Enumerable.Empty<Domains.Book.Models.Book>().ToList();
            }
            Assert.IsTrue(allBooks.Contains(book));
        }

        [Test]
        public async Task TestRemove()
        {
            var book = _bookFiller.FillBook();
            await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand { Book = book });
            var removeResponse = await _removeBookCommand.GetResponse<RemoveBookCommandResult>(
                new RemoveBookCommand { BookId = book.Id });
            Assert.IsTrue(removeResponse.Message.Success);
            var findResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery { BookId = book.Id });
            var findAllResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery());
            Domains.Book.Models.Book found = null;
            var allBooks = Enumerable.Empty<Domains.Book.Models.Book>().ToList();
            if (findResponse.Message.Results.HasValue)
            {
                var json = await findResponse.Message.Results.Value;
                var books = JsonConvert.DeserializeObject<List<Domains.Book.Models.Book>>(json);
                found = books?.SingleOrDefault();
            }
            if (findAllResponse.Message.Results.HasValue)
            {
                var json = await findResponse.Message.Results.Value;
                allBooks = JsonConvert.DeserializeObject<List<Domains.Book.Models.Book>>(json) ?? Enumerable.Empty<Domains.Book.Models.Book>().ToList();
            }
            Assert.IsNull(found);
            Assert.IsFalse(allBooks.Contains(book));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services?.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}