using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Models;
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
    public class SubjectTests
    {
        private IServiceProvider _services;
        private CompanyFiller _companyFiller;
        private PersonFiller _personFiller;
        
        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("PeopleContext");
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
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
            _companyFiller = new CompanyFiller();
            _personFiller = new PersonFiller();
        }

        [Test]
        public async Task TestSave()
        {
            Subject subject1 = _personFiller.FillPerson();
            Subject subject2 = _companyFiller.FillCompany();
            var subjects = _services.GetRequiredService<ISubjectRepository>();
            var saved1 = await subjects.Save(subject1);
            var saved2 = await subjects.Save(subject2);
            Assert.AreNotSame(subject1, saved1);
            Assert.AreEqual(subject1.Id, saved1.Id);
            Assert.AreEqual(subject2.Id, saved2.Id);
            Assert.AreEqual(subject1, saved1);
            Assert.AreEqual(subject2, saved2);
            // you can probably do some things like changing a Person to a Company that aren't
            // currently supported. This test suite doesn't try to catch those scenarios.
            subject1 = _personFiller.FillPerson();
            subject2 = _companyFiller.FillCompany();
            subject1.Id = saved1.Id;
            subject2.Id = saved2.Id;
            var updated1 = await subjects.Save(subject1);
            var updated2 = await subjects.Save(subject2);
            Assert.AreNotSame(subject1, updated1);
            Assert.AreEqual(subject1.Id, updated1.Id);
            Assert.AreEqual(subject1, updated1);
            Assert.AreEqual(subject2.Id, updated2.Id);
            Assert.AreEqual(subject2, updated2);
        }

        [Test]
        public async Task TestFind()
        {
            Subject subject1 = _personFiller.FillPerson();
            Subject subject2 = _companyFiller.FillCompany();
            var subjects = _services.GetRequiredService<ISubjectRepository>();
            subject1 = await subjects.Save(subject1);
            subject2 = await subjects.Save(subject2);
            var found1 = await subjects.Find(subject1.Id);
            var found2 = await subjects.Find(subject2.Id);
            var all = await subjects.FindAll();
            Assert.AreNotSame(subject1, found1);
            Assert.AreEqual(subject1.Id, found1.Id);
            Assert.AreEqual(subject1, found1);
            Assert.IsTrue(all.Contains(subject1));
            Assert.AreNotSame(subject2, found2);
            Assert.AreEqual(subject2.Id, found2.Id);
            Assert.AreEqual(subject2, found2);
            Assert.IsTrue(all.Contains(subject2));
        }

        [Test]
        public async Task TestRemove()
        {
            Subject subject1 = _companyFiller.FillCompany();
            Subject subject2 = _personFiller.FillPerson();
            var subjects = _services.GetRequiredService<ISubjectRepository>();
            subject1 = await subjects.Save(subject1);
            subject2 = await subjects.Save(subject2);
            var removed1 = await subjects.Remove(subject1.Id);
            var removed2 = await subjects.Remove(subject2.Id);
            Assert.IsTrue(removed1);
            Assert.IsTrue(removed2);
            var found1 = await subjects.Find(subject1.Id);
            var found2 = await subjects.Find(subject2.Id);
            var all = await subjects.FindAll();
            Assert.IsNull(found1);
            Assert.IsNull(found2);
            Assert.IsTrue(all.All(s => s.Id != subject1.Id));
            Assert.IsTrue(all.All(s => s.Id != subject2.Id));
            Assert.IsFalse(all.Contains(subject1));
            Assert.IsFalse(all.Contains(subject2));
        }
    }
}