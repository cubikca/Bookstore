using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Bookstore.ObjectFillers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Entities.People.Tests
{
    public class PersonTests
    {
        private IServiceProvider _services;
        private PersonFiller _personFiller;

        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("PeopleContext");
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseSqlServer(connectionString);
            });
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
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
            _personFiller = new PersonFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var person = _personFiller.FillPerson();
            var people = _services.GetRequiredService<IPersonRepository>();
            var created = await people.Save(person);
            Assert.AreNotSame(person, created);
            Assert.AreEqual(person.Id, created.Id);
            Assert.AreEqual(person, created);
            person = _personFiller.FillPerson();
            person.Id = created.Id;
            var updated = await people.Save(person);
            Assert.AreNotSame(person, updated);
            Assert.AreEqual(person.Id, updated.Id);
            Assert.AreEqual(person, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var person = _personFiller.FillPerson();
            var people = _services.GetRequiredService<IPersonRepository>();
            person = await people.Save(person);
            var found = await people.Find(person.Id);
            var all = await people.FindAll();
            Assert.NotNull(found);
            Assert.AreNotSame(person, found);
            Assert.AreEqual(person.Id, found.Id);
            Assert.AreEqual(person, found);
            Assert.IsTrue(all.Any(p => p.Id == person.Id));
            Assert.IsTrue(all.Contains(person));
        }

        [Test]
        public async Task TestRemove()
        {
            var person = _personFiller.FillPerson();
            var people = _services.GetRequiredService<IPersonRepository>();
            person = await people.Save(person);
            var removed = await people.Remove(person.Id);
            Assert.IsTrue(removed);
            var found = await people.Find(person.Id);
            var all = await people.FindAll();
            Assert.IsNull(found);
            Assert.IsTrue(all.All(p => p.Id != person.Id));
            Assert.IsFalse(all.Contains(person));
        }
    }
}