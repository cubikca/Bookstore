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
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Entities.People
{
    public class CountryTests
    {
        private ILogger<CountryRepository> _logger;
        private IMapper _mapper;
        private Filler<Country> _countryFiller;
        private Filler<Province> _provinceFiller;
        private ICountryRepository _countries;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer("Data Source=(local);Initial Catalog=PeopleCountryTests;User Id=dev;Password=development");
            });
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            using var loggerFactory = LoggerFactory.Create(cfg => cfg.AddConsole());
            _logger = loggerFactory.CreateLogger<CountryRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            _countries = new CountryRepository(dbFactory, _mapper, _logger);
            _countryFiller = new Filler<Country>();
            _provinceFiller = new Filler<Province>();
            await using var db = dbFactory.CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
       }

        [Test]
        public async Task TestSave()
        {
            var country = _countryFiller.Create();
            var created = await _countries.SaveCountry(country);
            Assert.AreNotSame(country, created);
            Assert.AreEqual(country, created);
            var province = _provinceFiller.Create();
            province.Country = created;
            var createdProvince = await _countries.SaveProvince(province);
            Assert.AreNotSame(province, createdProvince);
            Assert.AreEqual(province, createdProvince);
        }

        [Test]
        public async Task TestFind()
        {
            var country = _countryFiller.Create();
            country = await _countries.SaveCountry(country);
            var province = _provinceFiller.Create();
            province.Country = country;
            province = await _countries.SaveProvince(province);
            var found = await _countries.FindCountryById(country.Id);
            var foundProvince = await _countries.FindProvinceById(province.Id);
            var all = await _countries.FindAllCountries();
            var provinces = await _countries.FindProvincesByCountryId(country.Id);
            Assert.AreNotSame(country, found);
            Assert.AreEqual(country, found);
            Assert.IsTrue(all.Contains(found));
            Assert.AreNotSame(province, foundProvince);
            Assert.AreEqual(province, foundProvince);
            Assert.IsTrue(provinces.Contains(foundProvince));
        }

        [Test]
        public async Task TestRemove()
        {
            var country = _countryFiller.Create();
            country = await _countries.SaveCountry(country);
            var province = _provinceFiller.Create();
            province.Country = country;
            province = await _countries.SaveProvince(province);
            var provinceRemoved = await _countries.RemoveProvince(province.Id);
            Assert.IsTrue(provinceRemoved);
            var foundProvince = await _countries.FindProvinceById(province.Id);
            Assert.IsNull(foundProvince);
            var countryRemoved = await _countries.RemoveCountry(country.Id);
            Assert.IsTrue(countryRemoved);
            var found = await _countries.FindCountryById(country.Id);
            Assert.IsNull(found);
        }
    }
}
