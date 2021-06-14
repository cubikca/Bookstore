using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.ObjectFillers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Services.Book.Tests
{
    public class PublisherTests
    {
        private IServiceProvider _services;
        private IRequestClient<SavePublisherCommand> _savePublisherCommand;
        private IRequestClient<FindPublishersQuery> _findPublishersQuery;
        private IRequestClient<RemovePublisherCommand> _removePublisherQuery;
        private PublisherFiller _publisherFiller;
        
        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddMassTransit(mt =>
            {
                mt.UsingRabbitMq((_, rmq) =>
                {
                    var connectionString = config.GetConnectionString("BookService");
                    rmq.Host(new Uri(connectionString));
                    rmq.UseBsonSerializer();
                });
                mt.AddRequestClient<SavePublisherCommand>();
                mt.AddRequestClient<FindPublishersQuery>();
                mt.AddRequestClient<RemovePublisherCommand>();
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
            _removePublisherQuery = _services.GetRequiredService<IRequestClient<RemovePublisherCommand>>();
            _publisherFiller = new PublisherFiller();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var publisher = _publisherFiller.FillPublisher();
            var savePublisherResponse = await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand {Publisher = publisher});
            Assert.NotNull(savePublisherResponse.Message.Publisher);
            Assert.AreEqual(publisher.Id, savePublisherResponse.Message.Publisher.Id);
            Assert.AreEqual(publisher, savePublisherResponse.Message.Publisher);
            publisher = _publisherFiller.FillPublisher();
            publisher.Id = savePublisherResponse.Message.Publisher.Id;
            var updatePublisherResponse = await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand {Publisher = publisher});
            Assert.NotNull(updatePublisherResponse.Message.Publisher);
            Assert.AreEqual(publisher.Id, updatePublisherResponse.Message.Publisher.Id);
            Assert.AreEqual(publisher, updatePublisherResponse.Message.Publisher);
        }

        [Test]
        public async Task TestFind()
        {
            var publisher = _publisherFiller.FillPublisher();
            await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand {Publisher = publisher});
            var findPublisherResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery {PublisherId = publisher.Id});
            var allPublishersResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery());
            var foundPublisher = findPublisherResponse.Message.Results.SingleOrDefault();
            Assert.NotNull(foundPublisher);
            Assert.AreEqual(publisher, foundPublisher);
            foundPublisher = allPublishersResponse.Message.Results.SingleOrDefault(p => p.Id == publisher.Id);
            Assert.NotNull(foundPublisher);
            Assert.AreEqual(publisher, foundPublisher);
        }

        [Test]
        public async Task TestRemove()
        {
            var publisher = _publisherFiller.FillPublisher();
            await _savePublisherCommand.GetResponse<SavePublisherCommandResult>(
                new SavePublisherCommand {Publisher = publisher});
            var removePublisherResponse = await _removePublisherQuery.GetResponse<RemovePublisherCommandResult>(
                new RemovePublisherCommand {PublisherId = publisher.Id});
            var findPublisherResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery {PublisherId = publisher.Id});
            var allPublishersResponse = await _findPublishersQuery.GetResponse<FindPublishersQueryResult>(
                new FindPublishersQuery());
            Assert.IsTrue(removePublisherResponse.Message.Success);
            var foundPublisher = findPublisherResponse.Message.Results.SingleOrDefault();
            Assert.IsNull(foundPublisher);
            foundPublisher = allPublishersResponse.Message.Results.SingleOrDefault(p => p.Id == publisher.Id);
            Assert.IsNull(foundPublisher);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}