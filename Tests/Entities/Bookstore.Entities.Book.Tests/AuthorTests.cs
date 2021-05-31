using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book.AutoMapper;
using Bookstore.Entities.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Entities.Book.Tests
{
    [TestFixture]
    public class AuthorTests
    {
        private AuthorFiller _authorFiller = new AuthorFiller();
        private IAuthorRepository _authors;
        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<BookContext>(cfg =>
            {
                cfg.UseLazyLoadingProxies();
                cfg.UseSqlServer(
                    "Data Source=sqlserver;Initial Catalog=BookAuthorTests;User Id=brian;Password=development");
            });
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookProfile>();
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<PublisherProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetRequiredService<IDbContextFactory<BookContext>>();
            var db = dbFactory.CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            _authors = sp.GetRequiredService<IAuthorRepository>();
        }

        [Test]
        public async Task TestSave()
        {
            var author = _authorFiller.FillAuthor();
            var created = await _authors.SaveAuthor(author);
            Assert.AreNotSame(author, created);
            Assert.AreEqual(author, created);
        }

        [Test]
        public async Task TestFind()
        {
            var author1 = _authorFiller.FillAuthor();
            var author2 = _authorFiller.FillAuthor();
            author1 = await _authors.SaveAuthor(author1);
            author2 = await _authors.SaveAuthor(author2);
            var all = await _authors.FindAllAuthors();
            var found = await _authors.FindAuthorById(author1.Id);
            Assert.NotNull(found);
            Assert.AreNotSame(author1, found);
            Assert.AreEqual(author1, found);
            Assert.IsTrue(all.Contains(author1));
            Assert.IsTrue(all.Contains(author2));
        }

        [Test]
        public async Task TestRemove()
        {
            var author = _authorFiller.FillAuthor();
            await _authors.SaveAuthor(author);
            var deleted = await _authors.RemoveAuthor(author.Id);
            Assert.IsTrue(deleted);
            var found = await _authors.FindAuthorById(author.Id);
            Assert.IsNull(found);
        }
    }
}