using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bookstore.Domains.Book;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.ObjectFillers;
using Enchilada.Azure.BlobStorage;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using MassTransit.MessageData.Enchilada;
using MassTransit.MultiBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NUnit.Framework;

namespace Bookstore.Services.Book.Tests
{
    public class AuthorTests
    {
        private IServiceProvider _services;
        private AuthorFiller _authorFiller;
        private IRequestClient<SaveAuthorCommand> _saveAuthorCommand;
        private IRequestClient<FindAuthorsQuery> _findAuthorsQuery;
        private IRequestClient<RemoveAuthorCommand> _removeAuthorCommand;
        
        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var blobStorageAdapter = new BlobStorageAdapterConfiguration
            {
                ConnectionString = "",
                ContainerReference = "masstransit"
            };
            var messageDataRepository = new EnchiladaMessageDataRepositoryFactory().Create(blobStorageAdapter);
            services.AddSingleton<IMessageDataRepository>(messageDataRepository);
            services.AddLogging(log => log.AddConsole().SetMinimumLevel(LogLevel.Debug));
            services.AddMassTransit(mt =>
            {
                mt.AddRequestClient<SaveAuthorCommand>();
                mt.AddRequestClient<FindAuthorsQuery>();
                mt.AddRequestClient<RemoveAuthorCommand>();
                mt.UsingAzureServiceBus((ctx, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var booksConfig = config.GetSection("BooksService");
                    var booksConnection = $"sb://{booksConfig["ServiceBusNamespace"]}.servicebus.windows.net";
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(booksConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                            booksConfig["AccessKeyName"],
                            booksConfig["AccessKey"])
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
            _authorFiller = new AuthorFiller();
            _saveAuthorCommand = _services.GetRequiredService<IRequestClient<SaveAuthorCommand>>();
            _findAuthorsQuery = _services.GetRequiredService<IRequestClient<FindAuthorsQuery>>();
            _removeAuthorCommand = _services.GetRequiredService<IRequestClient<RemoveAuthorCommand>>();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var author = _authorFiller.FillAuthor();
            var saveAuthorResponse = await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand { Author = author });
            Assert.IsTrue(saveAuthorResponse.Message.Success);
            Assert.IsNotNull(saveAuthorResponse.Message.Author);
            Assert.AreEqual(author.Id, saveAuthorResponse.Message.Author.Id);
            Assert.AreEqual(author, saveAuthorResponse.Message.Author);
            author = _authorFiller.FillAuthor();
            author.Id = saveAuthorResponse.Message.Author.Id;
            var updateAuthorResponse = await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand { Author = author });
            Assert.IsTrue(updateAuthorResponse.Message.Success);
            Assert.IsNotNull(updateAuthorResponse.Message.Author);
            Assert.AreEqual(author.Id, updateAuthorResponse.Message.Author.Id);
            Assert.AreEqual(author, updateAuthorResponse.Message.Author);
        }

        [Test]
        public async Task TestFind()
        {
            var author = _authorFiller.FillAuthor();
            await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand { Author = author });
            var findAuthorResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery { AuthorId = author.Id });
            Author foundAuthor = null;
            if (findAuthorResponse.Message.Results.HasValue)
            {
                var json = await findAuthorResponse.Message.Results.Value;
                var authors = JsonConvert.DeserializeObject<List<Author>>(json);
                foundAuthor = authors.SingleOrDefault();
            }
            var allAuthorsResponse =
                await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(new FindAuthorsQuery());
            var allAuthors = Enumerable.Empty<Author>().ToList();
            if (allAuthorsResponse.Message.Results.HasValue)
            {
                var json = await allAuthorsResponse.Message.Results.Value;
                allAuthors = JsonConvert.DeserializeObject<List<Author>>(json) ?? Enumerable.Empty<Author>().ToList();
            }
            Assert.AreEqual(author, foundAuthor);
            Assert.IsTrue(allAuthors.Contains(author));
        }

        [Test]
        public async Task TestRemove()
        {
            var author = _authorFiller.FillAuthor();
            var saveResponse = await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand { Author = author });
            var removeAuthorResponse = await _removeAuthorCommand.GetResponse<RemoveAuthorCommandResult>(
                new RemoveAuthorCommand { AuthorId = author.Id });
            Assert.IsTrue(removeAuthorResponse.Message.Success);
            var foundAuthorResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery { AuthorId = saveResponse.Message.Author.Id });
            var allAuthorsResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery());
            Author foundAuthor = null;
            var allAuthors = Enumerable.Empty<Author>().ToList();
            if (foundAuthorResponse.Message.Results.HasValue)
            {
                var json = await foundAuthorResponse.Message.Results.Value;
                var authors = JsonConvert.DeserializeObject<List<Author>>(json);
                foundAuthor = authors?.SingleOrDefault();
            }
            if (allAuthorsResponse.Message.Results.HasValue)
            {
                var json = await allAuthorsResponse.Message.Results.Value;
                allAuthors = JsonConvert.DeserializeObject<List<Author>>(json) ?? Enumerable.Empty<Author>().ToList();
            }
            Assert.IsNull(foundAuthor);
            Assert.IsTrue(allAuthors.All(r => !Equals(r, author)));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}