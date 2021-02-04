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
    public class PublisherTests
    {
        private BookContext _db;
        private PublisherFiller _publisherFiller;
        private IPublisherRepository _publishers;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _publisherFiller = new PublisherFiller();
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<BookContext>(cfg =>
            {
                cfg.UseLazyLoadingProxies();
                cfg.UseSqlServer(
                    "Data Source=(local);Initial Catalog=BookPublisherTests;User Id=brian;Password=development");
            });
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookProfile>();
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<PublisherProfile>();
            });
            services.AddSingleton(mapperConfig.CreateMapper());
            services.AddScoped<IPublisherRepository, PublisherRepository>();
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetRequiredService<IDbContextFactory<BookContext>>();
            _db = dbFactory.CreateDbContext();
            _publishers = sp.GetRequiredService<IPublisherRepository>();
            await _db.Database.EnsureDeletedAsync();
            await _db.Database.MigrateAsync();
        }

        [Test]
        public async Task TestSave()
        {
            var publisher = _publisherFiller.FillPublisher();
            var created = await _publishers.SavePublisher(publisher);
            Assert.NotNull(created);
            Assert.AreNotSame(created, publisher);
            Assert.AreEqual(created, publisher);
        }

        [Test]
        public async Task TestFind()
        {
            var publisher1 = _publisherFiller.FillPublisher();
            var publisher2 = _publisherFiller.FillPublisher();
            publisher1 = await _publishers.SavePublisher(publisher1);
            publisher2 = await _publishers.SavePublisher(publisher2);
            var all = await _publishers.FindAllPublishers();
            var found = await _publishers.FindPublisherById(publisher1.Id);
            Assert.AreNotSame(publisher1, found);
            Assert.AreEqual(publisher1, found);
            Assert.IsTrue(all.Contains(publisher1));
            Assert.IsTrue(all.Contains(publisher2));
        }

        [Test]
        public async Task TestRemove()
        {
            var publisher = _publisherFiller.FillPublisher();
            publisher = await _publishers.SavePublisher(publisher);
            var deleted = await _publishers.RemovePublisher(publisher.Id);
            Assert.IsTrue(deleted);
            var found = await _publishers.FindPublisherById(publisher.Id);
            Assert.IsNull(found);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_db != null) await _db.DisposeAsync();
        }
    }
}