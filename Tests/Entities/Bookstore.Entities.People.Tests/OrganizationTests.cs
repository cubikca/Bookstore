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
    public class OrganizationTests
    {
        private IServiceProvider _services;
        private OrganizationFiller _organizationFiller;

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
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
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
            _organizationFiller = new OrganizationFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var organization = _organizationFiller.FillOrganization();
            var organizations = _services.GetRequiredService<IOrganizationRepository>();
            var saved = await organizations.Save(organization);
            Assert.AreNotSame(organization, saved);
            Assert.AreEqual(organization.Id, saved.Id);
            Assert.AreEqual(organization, saved);
            organization = _organizationFiller.FillOrganization();
            organization.Id = saved.Id;
            var updated = await organizations.Save(organization);
            Assert.AreNotSame(organization, updated);
            Assert.AreEqual(organization.Id, updated.Id);
            Assert.AreEqual(organization, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var organization = _organizationFiller.FillOrganization();
            var organizations = _services.GetRequiredService<IOrganizationRepository>();
            organization = await organizations.Save(organization);
            var found = await organizations.Find(organization.Id);
            var all = await organizations.FindAll();
            Assert.NotNull(found);
            Assert.AreEqual(organization.Id, found.Id);
            Assert.AreEqual(organization, found);
            Assert.IsTrue(all.Any(c => c.Id == organization.Id));
            Assert.IsTrue(all.Contains(organization));
        }

        [Test]
        public async Task TestRemove()
        {
            var organization = _organizationFiller.FillOrganization();
            var organizations = _services.GetRequiredService<IOrganizationRepository>();
            organization = await organizations.Save(organization);
            var removed = await organizations.Remove(organization.Id);
            var found = await organizations.Find(organization.Id);
            var all = await organizations.FindAll();
            Assert.IsTrue(removed);
            Assert.IsNull(found);
            Assert.IsTrue(all.All(c => c.Id != organization.Id));
            Assert.IsFalse(all.Contains(organization));
        }
    }
}