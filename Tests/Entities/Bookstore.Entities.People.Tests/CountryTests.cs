using System;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.People.Tests
{
    public class CountryTests
    {
        private IMapper _mapper;
        // we want to test all three interfaces, so use the class type instead
        private AddressRepository _addresses;
        private Filler<Country> _countryFiller;
        private Filler<Province> _provinceFiller;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = "server=localhost;user=brian;password=development;database=PeopleEntitiesTests";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            using var loggerFactory = LoggerFactory.Create(cfg => cfg.AddConsole());
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            _addresses = new AddressRepository(dbFactory, _mapper, sp.GetService<ILogger<AddressRepository>>());
            _countryFiller = new Filler<Country>();
            _provinceFiller = new Filler<Province>();
            await using var db = dbFactory.CreateDbContext();
       }

        [Test]
        public async Task TestSave()
        {
            var country = _countryFiller.Create();
            var created = await _addresses.SaveCountry(country);
            Assert.AreNotSame(country, created);
            Assert.AreEqual(country, created);
            var province = _provinceFiller.Create();
            province.Country = created;
            var createdProvince = await _addresses.SaveProvince(province);
            Assert.AreNotSame(province, createdProvince);
            Assert.AreEqual(province, createdProvince);
        }

        [Test]
        public async Task TestFind()
        {
            var country = _countryFiller.Create();
            country = await _addresses.SaveCountry(country);
            var province = _provinceFiller.Create();
            province.Country = country;
            province = await _addresses.SaveProvince(province);
            var found = await _addresses.FindCountryByAbbreviation(country.Abbreviation);
            var foundProvince = await _addresses.FindProvinceByAbbreviation(province.Abbreviation);
            var all = await _addresses.FindAllCountries();
            var provinces = await _addresses.FindProvincesByCountryAbbreviation(country.Abbreviation);
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
            country = await _addresses.SaveCountry(country);
            var province = _provinceFiller.Create();
            province.Country = country;
            province = await _addresses.SaveProvince(province);
            var provinceRemoved = await _addresses.RemoveProvince(province.Abbreviation);
            Assert.IsTrue(provinceRemoved);
            var foundProvince = await _addresses.FindProvinceByAbbreviation(province.Abbreviation);
            Assert.IsNull(foundProvince);
            var countryRemoved = await _addresses.RemoveCountry(country.Abbreviation);
            Assert.IsTrue(countryRemoved);
            var found = await _addresses.FindCountryByAbbreviation(country.Abbreviation);
            Assert.IsNull(found);
        }
    }
}
