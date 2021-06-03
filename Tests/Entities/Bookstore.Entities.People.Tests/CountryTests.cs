using System;
using System.IO;
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
    public class CountryTests
    {
        private CountryFiller _countryFiller;
        private IServiceProvider _services;

        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddLogging(cfg => cfg.AddConsole());
            var connectionString = config.GetConnectionString("PeopleContext");
            services.AddDbContextFactory<PeopleContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddScoped<ICountryRepository, CountryRepository>();
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build();
            ConfigureServices(services, config);
            _services = services.BuildServiceProvider();
            _countryFiller = new CountryFiller();
       }

        [Test]
        public async Task TestSave()
        {
            var countries = _services.GetRequiredService<ICountryRepository>();
            var country = _countryFiller.FillCountry();
            var created = await countries.Save(country);
            Assert.AreNotSame(country, created);
            Assert.AreEqual(country, created);
            country = _countryFiller.FillCountry();
            country.Id = created.Id;
            var updated = await countries.Save(country);
            Assert.AreNotSame(country, updated);
            Assert.AreEqual(country, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var countries = _services.GetRequiredService<ICountryRepository>();
            var country = _countryFiller.FillCountry();
            country = await countries.Save(country);
            var found = await countries.Find(country.Id);
            var all = await countries.FindAll();
            Assert.AreNotSame(country, found);
            Assert.AreEqual(country, found);
            Assert.IsTrue(all.Contains(found));
        }

        [Test]
        public async Task TestRemove()
        {
            var countries = _services.GetRequiredService<ICountryRepository>();
            var country = _countryFiller.FillCountry();
            country = await countries.Save(country);
            var removed = await countries.Remove(country.Id);
            Assert.IsTrue(removed);
            var found = await countries.Find(country.Id);
            Assert.IsNull(found);
        }
    }
}
