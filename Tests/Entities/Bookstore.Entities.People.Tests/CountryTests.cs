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
        private IProvinceRepository _provinces;
        private ICountryRepository _countries;
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
            _provinces = new ProvinceRepository(dbFactory, _mapper, sp.GetService<ILogger<ProvinceRepository>>());
            _countries = new CountryRepository(dbFactory, _mapper, _provinces, sp.GetService<ILogger<CountryRepository>>());
            _countryFiller = new Filler<Country>();
            _provinceFiller = new Filler<Province>();
            await using var db = dbFactory.CreateDbContext();
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
            var createdProvince = await _provinces.SaveProvince(province);
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
            province = await _provinces.SaveProvince(province);
            var found = await _countries.FindCountryById(country.Id);
            var foundProvince = await _provinces.FindProvinceById(province.Id);
            var all = await _countries.FindAllCountries();
            var provinces = await _provinces.FindProvincesByCountryId(country.Id);
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
            province = await _provinces.SaveProvince(province);
            var provinceRemoved = await _provinces.RemoveProvince(province.Id);
            Assert.IsTrue(provinceRemoved);
            var foundProvince = await _provinces.FindProvinceById(province.Id);
            Assert.IsNull(foundProvince);
            var countryRemoved = await _countries.RemoveCountry(country.Id);
            Assert.IsTrue(countryRemoved);
            var found = await _countries.FindCountryById(country.Id);
            Assert.IsNull(found);
        }
    }
}
