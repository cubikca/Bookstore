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
    public class LocationTests
    {
        private IServiceProvider _services;
        private LocationFiller _locationFiller;
        
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
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
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
            _locationFiller = new LocationFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var location = _locationFiller.FillLocation();
            var locations = _services.GetRequiredService<ILocationRepository>();
            var saved = await locations.Save(location);
            Assert.AreNotSame(location, saved);
            Assert.AreEqual(location.Id, saved.Id);
            Assert.AreEqual(location, saved);
            location = _locationFiller.FillLocation();
            location.Id = saved.Id;
            var updated = await locations.Save(location);
            Assert.AreNotSame(location, updated);
            Assert.AreEqual(location.Id, updated.Id);
            Assert.AreEqual(location, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var location = _locationFiller.FillLocation();
            var locations = _services.GetRequiredService<ILocationRepository>();
            location = await locations.Save(location);
            var found = await locations.Find(location.Id);
            var all = await locations.FindAll();
            Assert.AreNotSame(location, found);
            Assert.AreEqual(location.Id, found.Id);
            Assert.AreEqual(location, found);
            Assert.IsTrue(all.Any(l => l.Id == location.Id));
            Assert.IsTrue(all.Contains(location));
        }

        [Test]
        public async Task TestRemove()
        {
            var location = _locationFiller.FillLocation();
            var locations = _services.GetRequiredService<ILocationRepository>();
            location = await locations.Save(location);
            var removed = await locations.Remove(location.Id);
            var found = await locations.Find(location.Id);
            var all = await locations.FindAll();
            Assert.IsNull(found);
            Assert.IsTrue(all.All(l => l.Id != location.Id));
            Assert.IsFalse(all.Contains(location));
        }
    }
}