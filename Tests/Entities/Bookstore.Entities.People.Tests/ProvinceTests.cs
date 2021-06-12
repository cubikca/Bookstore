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
    public class ProvinceTests
    {
        private IServiceProvider _services;
        private ProvinceFiller _provinceFiller;
        
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
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
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
            _provinceFiller = new ProvinceFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var provinces = _services.GetRequiredService<IProvinceRepository>();
            var province = _provinceFiller.FillProvince();
            var created = await provinces.Save(province);
            Assert.AreNotSame(province, created);
            Assert.AreEqual(province.Id, created.Id);
            Assert.AreEqual(province, created);
            province = _provinceFiller.FillProvince();
            province.Id = created.Id;
            var updated = await provinces.Save(province);
            Assert.AreNotSame(province, updated);
            Assert.AreEqual(province.Id, updated.Id);
            Assert.AreEqual(province, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var provinces = _services.GetRequiredService<IProvinceRepository>();
            var province = _provinceFiller.FillProvince();
            province = await provinces.Save(province);
            var found = await provinces.Find(province.Id);
            var all = await provinces.FindAll();
            Assert.NotNull(found);
            Assert.AreEqual(province.Id, found.Id);
            Assert.AreEqual(province, found);
            Assert.IsTrue(all.Any(p => p.Id == found.Id));
            Assert.IsTrue(all.Contains(found));
        }

        [Test]
        public async Task TestRemove()
        {
            var provinces = _services.GetRequiredService<IProvinceRepository>();
            var province = _provinceFiller.FillProvince();
            province = await provinces.Save(province);
            var removed = await provinces.Remove(province.Id);
            Assert.IsTrue(removed);
            var found = await provinces.Find(province.Id);
            Assert.IsNull(found);
        }
    }
}