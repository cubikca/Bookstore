using System;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Repositories;
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
        private IMapper _mapper;
        private IPersonRepository _people;
        private IProvinceRepository _provinces;
        private ICountryRepository _countries;
        private IAddressRepository _addresses;
        private PersonFiller _personFiller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = "server=localhost;user=brian;password=development;database=PeopleEntitiesTests";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            _provinces = new ProvinceRepository(dbFactory, _mapper, sp.GetService<ILogger<ProvinceRepository>>());
            _countries = new CountryRepository(dbFactory, _mapper, _provinces,
                sp.GetService<ILogger<CountryRepository>>());
            _addresses = new AddressRepository(dbFactory, _mapper, _countries, _provinces, sp.GetService<ILogger<AddressRepository>>());
            _people = new PersonRepository(dbFactory, _mapper, _addresses, sp.GetService<ILogger<PersonRepository>>());
            _personFiller = new PersonFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var person = _personFiller.FillPerson();
            var created = await _people.SavePerson(person);
            Assert.AreNotSame(person, created);
            Assert.AreEqual(person, created);
            created = _personFiller.FillPerson();
            created.Id = person.Id;
            var updated = await _people.SavePerson(created);
            Assert.AreNotSame(created, updated);
            Assert.AreEqual(created, updated);
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
