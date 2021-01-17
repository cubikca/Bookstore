using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Entities.People.Tests
{
    [TestFixture]
    public class PersonTests
    {
        private ILogger<PersonRepository> _logger;
        private IMapper _mapper;
        private IPersonRepository _people;
        private PersonFiller _personFiller;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer(
                        "Data Source=(local);Initial Catalog=PeoplePersonTests;User Id=brian;Password=development");
            });
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            using var loggerFactory = LoggerFactory.Create(cfg => cfg.AddConsole());
            _logger = loggerFactory.CreateLogger<PersonRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            _people = new PersonRepository(dbFactory, _mapper, _logger);
            _personFiller = new PersonFiller();
            await using var db = dbFactory.CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        [Test]
        public async Task TestSave()
        {
            var person = _personFiller.FillPerson();
            var created = await _people.SavePerson(person);
            Assert.AreNotSame(person, created);
            Assert.AreEqual(person, created);
        }

        [Test]
        public async Task TestFind()
        {
            var person = _personFiller.FillPerson();
            var created = await _people.SavePerson(person);
            var found = await _people.FindPersonById(created.Id);
            var all = await _people.FindAllPeople();
            Assert.AreNotSame(created, found);
            Assert.AreEqual(created, found);
            Assert.IsTrue(all.Contains(created));
        }

        [Test]
        public async Task TestRemove()
        {
            var person = _personFiller.FillPerson();
            var created = await _people.SavePerson(person);
            var removed = await _people.RemovePerson(created.Id);
            Assert.IsTrue(removed);
            var found = await _people.FindPersonById(created.Id);
            Assert.IsNull(found);
        }
    }
}
