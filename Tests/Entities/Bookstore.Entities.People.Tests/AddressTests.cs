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
    public class AddressTests
    {
        private IServiceProvider _services;
        private AddressFiller _addressFiller;

        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("PeopleContext");
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(options =>
            {
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
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
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
            _addressFiller = new AddressFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var addresses = _services.GetRequiredService<IAddressRepository>();
            var address = _addressFiller.FillAddress();
            var created = await addresses.Save(address);
            Assert.AreNotSame(address, created);
            Assert.AreEqual(address.Id, created.Id);
            Assert.AreEqual(address, created);
            address = _addressFiller.FillAddress();
            address.Id = created.Id;
            var updated = await addresses.Save(address);
            Assert.AreNotSame(address, updated);
            Assert.AreEqual(address.Id, updated.Id);
            Assert.AreEqual(address, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var addresses = _services.GetRequiredService<IAddressRepository>();
            var address = _addressFiller.FillAddress();
            address = await addresses.Save(address);
            var found = await addresses.Find(address.Id);
            var all = await addresses.FindAll();
            Assert.AreNotSame(address, found);
            Assert.AreEqual(address.Id, found.Id);
            Assert.AreEqual(address, found);
            Assert.IsTrue(all.Any(a => a.Id == address.Id));
            Assert.IsTrue(all.Contains(address));
        }

        [Test]
        public async Task TestRemove()
        {
            var addresses = _services.GetRequiredService<IAddressRepository>();
            var address = _addressFiller.FillAddress();
            address = await addresses.Save(address);
            var removed = await addresses.Remove(address.Id);
            Assert.IsTrue(removed);
            var found = await addresses.Find(address.Id);
            Assert.IsNull(found);
        }
    }
}