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
    public class AuthorTests
    {
        private IServiceProvider _services;
        private IRequestClient<SaveAuthorCommand> _saveAuthorCommand;
        private IRequestClient<FindAuthorsQuery> _findAuthorsQuery;
        private IRequestClient<RemoveAuthorCommand> _removeAuthorCommand;
        private AuthorFiller _authorFiller;
        
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
                mt.AddRequestClient<SaveAuthorCommand>();
                mt.AddRequestClient<FindAuthorsQuery>();
                mt.AddRequestClient<RemoveAuthorCommand>();
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
            _saveAuthorCommand = _services.GetRequiredService<IRequestClient<SaveAuthorCommand>>();
            _findAuthorsQuery = _services.GetRequiredService<IRequestClient<FindAuthorsQuery>>();
            _removeAuthorCommand = _services.GetRequiredService<IRequestClient<RemoveAuthorCommand>>();
            _authorFiller = new AuthorFiller();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var author = _authorFiller.FillAuthor();
            var saveAuthorResponse = await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand {Author = author});
            Assert.NotNull(saveAuthorResponse.Message.Author);
            Assert.AreEqual(author.Id, saveAuthorResponse.Message.Author.Id);
            Assert.AreEqual(author, saveAuthorResponse.Message.Author);
            author = _authorFiller.FillAuthor();
            author.Id = saveAuthorResponse.Message.Author.Id;
            var updateAuthorResponse = await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand {Author = author});
            Assert.NotNull(updateAuthorResponse.Message.Author);
            Assert.AreEqual(author.Id, updateAuthorResponse.Message.Author.Id);
            Assert.AreEqual(author, updateAuthorResponse.Message.Author);
        }

        [Test]
        public async Task TestFind()
        {
            var author = _authorFiller.FillAuthor();
            await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand {Author = author});
            var findAuthorResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery {AuthorId = author.Id});
            var allAuthorsResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery());
            var foundAuthor = findAuthorResponse.Message.Results.SingleOrDefault();
            Assert.NotNull(foundAuthor);
            Assert.AreEqual(author.Id, foundAuthor.Id);
            Assert.AreEqual(author, foundAuthor);
            Assert.IsTrue(allAuthorsResponse.Message.Results.Any(a => a.Id == author.Id));
            var authorMatch = allAuthorsResponse.Message.Results.Single(a => a.Id == author.Id);
            Assert.AreEqual(author, authorMatch);
        }

        [Test]
        public async Task TestRemove()
        {
            var author = _authorFiller.FillAuthor();
            await _saveAuthorCommand.GetResponse<SaveAuthorCommandResult>(
                new SaveAuthorCommand {Author = author});
            var removeAuthorResponse = await _removeAuthorCommand.GetResponse<RemoveAuthorCommandResult>(
                new RemoveAuthorCommand {AuthorId = author.Id});
            Assert.IsTrue(removeAuthorResponse.Message.Success);
            var findAuthorResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery {AuthorId = author.Id});
            var allAuthorsResponse = await _findAuthorsQuery.GetResponse<FindAuthorsQueryResult>(
                new FindAuthorsQuery());
            var foundAuthor = findAuthorResponse.Message.Results.SingleOrDefault();
            Assert.IsNull(foundAuthor);
            Assert.IsTrue(allAuthorsResponse.Message.Results.All(r => r.Id != author.Id));
            Assert.IsFalse(allAuthorsResponse.Message.Results.Contains(author));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}