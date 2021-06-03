using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.Book.Models;
using Bookstore.Entities.Book.AutoMapper;
using Bookstore.Entities.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Entities.Book.Tests
{
    public class BookTests
    {
        private IBookRepository _books;
        private BookFiller _bookFiller;
        
        [OneTimeSetUp]
        public async Task Setup()
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg =>
            {
                cfg.AddConsole();
            });
            services.AddDbContextFactory<BookContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.EnableSensitiveDataLogging();
                var connectionString = "server=mysql;user=brian;password=development;database=BooksEntitiesTests";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddScoped<IBookRepository, BookRepository>();
            _bookFiller = new BookFiller();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookProfile>();
                cfg.AddProfile<PublisherProfile>();
                cfg.AddProfile<AuthorProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            var sp = services.BuildServiceProvider();
            _books = sp.GetService<IBookRepository>();
            var dbFactory = sp.GetService<IDbContextFactory<BookContext>>();
            Assert.NotNull(dbFactory);
            var db = dbFactory.CreateDbContext();
        }

        [Test]
        public async Task TestFind()
        {
            var book = _bookFiller.FillBook();
            var created = await _books.SaveBook(book);
            var found = await _books.FindBookById(created.Id);
            var all = await _books.FindAllBooks();
            Assert.NotNull(found);
            Assert.AreNotSame(created, found);
            Assert.AreEqual(created, found);
            Assert.IsTrue(all.Contains(created));
        }

        [Test]
        public async Task TestSave()
        {
            var book = _bookFiller.FillBook();
            var created = await _books.SaveBook(book);
            Assert.AreNotSame(book, created);
            Assert.AreEqual(book, created);
            Assert.AreEqual(book.Authors, created.Authors);
            Assert.AreEqual(book.Publisher, created.Publisher);
        }

        [Test]
        public async Task TestRemove()
        {
            var book = _bookFiller.FillBook();
            var created = await _books.SaveBook(book);
            var deleted = await _books.RemoveBook(created.Id);
            Assert.IsTrue(deleted);
            var found = await _books.FindBookById(created.Id);
            Assert.Null(found);
        }
    }
}