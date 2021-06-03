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
    public class CompanyTests
    {
        private IServiceProvider _services;
        private CompanyFiller _companyFiller;

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
        }

        [Test]
        public async Task TestSave()
        {
            var company = _companyFiller.FillCompany();
            var companies = _services.GetRequiredService<ICompanyRepository>();
            var saved = await companies.Save(company);
            Assert.AreNotSame(company, saved);
            Assert.AreEqual(company.Id, saved.Id);
            Assert.AreEqual(company, saved);
            company = _companyFiller.FillCompany();
            company.Id = saved.Id;
            var updated = await companies.Save(company);
            Assert.AreNotSame(company, updated);
            Assert.AreEqual(company.Id, updated.Id);
            Assert.AreEqual(company, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var company = _companyFiller.FillCompany();
            var companies = _services.GetRequiredService<ICompanyRepository>();
            company = await companies.Save(company);
            var found = await companies.Find(company.Id);
            var all = await companies.FindAll();
            Assert.NotNull(found);
            Assert.AreEqual(company.Id, found.Id);
            Assert.AreEqual(company, found);
            Assert.IsTrue(all.Any(c => c.Id == company.Id));
            Assert.IsTrue(all.Contains(company));
        }

        [Test]
        public async Task TestRemove()
        {
            var company = _companyFiller.FillCompany();
            var companies = _services.GetRequiredService<ICompanyRepository>();
            company = await companies.Save(company);
            var removed = await companies.Remove(company.Id);
            var found = await companies.Find(company.Id);
            var all = await companies.FindAll();
            Assert.IsTrue(removed);
            Assert.IsNull(found);
            Assert.IsTrue(all.All(c => c.Id != company.Id));
            Assert.IsFalse(all.Contains(company));
        }
    }
}