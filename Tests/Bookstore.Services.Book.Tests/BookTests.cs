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
    public class BookTests
    {
        private IServiceProvider _services;
        private IRequestClient<SaveBookCommand> _saveBookCommand;
        private IRequestClient<FindBooksQuery> _findBooksQuery;
        private IRequestClient<RemoveBookCommand> _removeBookCommand;
        private BookFiller _bookFiller;
        
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
                mt.AddRequestClient<SaveBookCommand>();
                mt.AddRequestClient<FindBooksQuery>();
                mt.AddRequestClient<RemoveBookCommand>();
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
            _bookFiller = new BookFiller();
            _saveBookCommand = _services.GetRequiredService<IRequestClient<SaveBookCommand>>();
            _findBooksQuery = _services.GetRequiredService<IRequestClient<FindBooksQuery>>();
            _removeBookCommand = _services.GetRequiredService<IRequestClient<RemoveBookCommand>>();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var book = _bookFiller.FillBook();
            var saveBookResponse = await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand {Book = book});
            Assert.NotNull(saveBookResponse.Message.Book);
            Assert.AreEqual(book.Id, saveBookResponse.Message.Book.Id);
            Assert.AreEqual(book, saveBookResponse.Message.Book);
            book = _bookFiller.FillBook();
            book.Id = saveBookResponse.Message.Book.Id;
            var updateBookResponse = await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand {Book = book});
            Assert.NotNull(updateBookResponse.Message.Book);
            Assert.AreEqual(book.Id, updateBookResponse.Message.Book.Id);
            Assert.AreEqual(book, updateBookResponse.Message.Book);
        }

        [Test]
        public async Task TestFind()
        {
            var book = _bookFiller.FillBook();
            await _saveBookCommand.GetResponse<SaveBookCommandResult>(
                new SaveBookCommand {Book = book});
            var findBookResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery {BookId = book.Id});
            var allBooksResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery());
            var foundBook = findBookResponse.Message.Results.SingleOrDefault();
            Assert.NotNull(foundBook);
            Assert.AreEqual(book.Id, foundBook.Id);
            Assert.AreEqual(book, foundBook);
            foundBook = allBooksResponse.Message.Results.SingleOrDefault(b => b.Id == book.Id);
            Assert.AreEqual(book, foundBook);
        }

        [Test]
        public async Task TestRemove()
        {
            var book = _bookFiller.FillBook();
            await _saveBookCommand.GetResponse<SaveBookCommandResult>(new SaveBookCommand {Book = book});
            var removeBookResponse = await _removeBookCommand.GetResponse<RemoveBookCommandResult>(
                new RemoveBookCommand {BookId = book.Id});
            Assert.IsTrue(removeBookResponse.Message.Success);
            var findBookResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery {BookId = book.Id});
            var allBooksResponse = await _findBooksQuery.GetResponse<FindBooksQueryResult>(
                new FindBooksQuery());
            var foundBook = findBookResponse.Message.Results.SingleOrDefault();
            Assert.IsNull(foundBook);
            Assert.IsTrue(allBooksResponse.Message.Results.All(b => b.Id != book.Id));
            Assert.IsFalse(allBooksResponse.Message.Results.Contains(book));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}