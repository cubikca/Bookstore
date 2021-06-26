using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.ObjectFillers;
using Enchilada.Azure.BlobStorage;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.Azure.Storage.MessageData;
using MassTransit.MessageData;
using MassTransit.MessageData.Enchilada;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NUnit.Framework;

namespace Bookstore.Services.Book.Tests
{
    public class PublisherTests
    {
        private IServiceProvider _services;
        private IRequestClient<SavePublisherCommand> _savePublisherCommand;
        private IRequestClient<FindPublishersQuery> _findPublishersQuery;
        private IRequestClient<RemovePublisherCommand> _removePublisherCommand;
        private PublisherFiller _publisherFiller;
        
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
                mt.AddRequestClient<SavePublisherCommand>();
                mt.AddRequestClient<FindPublishersQuery>();
                mt.AddRequestClient<RemovePublisherCommand>();
                mt.UsingAzureServiceBus((_, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var booksConfig = config.GetSection("BooksService");
                    var booksConnection = $"sb://{booksConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                    var secretName = booksConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(booksConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                            booksConfig["AccessKeyName"], sharedAccessKey)
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
            _savePublisherCommand = _services.GetRequiredService<IRequestClient<SavePublisherCommand>>();
            _findPublishersQuery = _services.GetRequiredService<IRequestClient<FindPublishersQuery>>();
            _removePublisherCommand = _services.GetRequiredService<IRequestClient<RemovePublisherCommand>>();
            _publisherFiller = new PublisherFiller();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var publisher = _publisherFiller.FillPublisher();
            var saveResponse = await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand { Publisher = publisher });
            Assert.IsNotNull(saveResponse.Message.Publisher);
            Assert.AreEqual(publisher.Id, saveResponse.Message.Publisher.Id);
            Assert.AreEqual(publisher, saveResponse.Message.Publisher);
            publisher = _publisherFiller.FillPublisher();
            publisher.Id = saveResponse.Message.Publisher.Id;
            var updateResponse = await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand { Publisher = publisher });
            Assert.IsNotNull(updateResponse.Message.Publisher);
            Assert.AreEqual(publisher.Id, updateResponse.Message.Publisher.Id);
            Assert.AreEqual(publisher, updateResponse.Message.Publisher);
        }

        [Test]
        public async Task TestFind()
        {
            var publisher = _publisherFiller.FillPublisher();
            await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand { Publisher = publisher });
            var findResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery { PublisherId = publisher.Id });
            Publisher found = null;
            if (findResponse.Message.Results.HasValue)
            {
                var json = await findResponse.Message.Results.Value;
                var publishers = JsonConvert.DeserializeObject<List<Publisher>>(json);
                found = publishers?.SingleOrDefault();
            }
            Assert.NotNull(found);
            Assert.AreEqual(publisher, found);
            var findAllResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery());
            var allPublishers = Enumerable.Empty<Publisher>().ToList();
            if (findAllResponse.Message.Results.HasValue)
            {
                var json = await findAllResponse.Message.Results.Value;
                allPublishers = JsonConvert.DeserializeObject<List<Publisher>>(json) ??
                                Enumerable.Empty<Publisher>().ToList();
            }
            Assert.IsTrue(allPublishers.Contains(publisher));
        }

        [Test]
        public async Task TestRemove()
        {
            var publisher = _publisherFiller.FillPublisher();
            await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand { Publisher = publisher });
            var removeResponse = await _removePublisherCommand.GetResponse<RemovePublisherCommandResult>(
                new RemovePublisherCommand { PublisherId = publisher.Id });
            Assert.IsTrue(removeResponse.Message.Success);
            Publisher found = null;
            var findResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery { PublisherId = publisher.Id });
            if (findResponse.Message.Results.HasValue)
            {
                var json = await findResponse.Message.Results.Value;
                var publishers = JsonConvert.DeserializeObject<List<Publisher>>(json);
                found = publishers?.SingleOrDefault();
            }
            Assert.IsNull(found);
            var findAllResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery());
            var allPublishers = Enumerable.Empty<Publisher>().ToList();
            if (findAllResponse.Message.Results.HasValue)
            {
                var json = await findAllResponse.Message.Results.Value;
                var publishers = JsonConvert.DeserializeObject<List<Publisher>>(json);
                allPublishers = publishers ?? Enumerable.Empty<Publisher>().ToList();
            }
            Assert.IsTrue(allPublishers.All(r => !Equals(publisher, r)));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services?.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}