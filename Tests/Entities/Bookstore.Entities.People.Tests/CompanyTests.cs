using System;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bookstore.Entities.People.Tests
{
    [TestFixture]
    public class CompanyTests
    {
        private ICompanyRepository _companies;
        private IMapper _mapper;
        private CompanyFiller _companyFiller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = "server=mysql;user=brian;password=development;database=PeopleEntitiesTests";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddLogging(opt => opt.AddConsole());
            var sp = services.BuildServiceProvider();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            var addresses = new AddressRepository(sp.GetService<IDbContextFactory<PeopleContext>>(), _mapper, sp.GetService<ILogger<AddressRepository>>());
            var people = new PersonRepository(sp.GetService<IDbContextFactory<PeopleContext>>(), _mapper, addresses, sp.GetService<ILogger<PersonRepository>>());
            _companies = new CompanyRepository(sp.GetService<IDbContextFactory<PeopleContext>>(), _mapper, addresses, people, sp.GetService<ILogger<CompanyRepository>>());
            _companyFiller = new CompanyFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var company = _companyFiller.FillCompany();
            var created = await _companies.SaveCompany(company);
            Assert.AreNotSame(company, created);
            Assert.AreEqual(company, created);
            created = _companyFiller.FillCompany();
            created.Id = company.Id;
            var updated = await _companies.SaveCompany(created);
            Assert.AreNotSame(company, updated);
            Assert.AreEqual(created, updated);
        }

        [Test]
        public async Task TestFind()
        {
            var company = _companyFiller.FillCompany();
            var created = await _companies.SaveCompany(company);
            var found = await _companies.FindCompanyById(created.Id);
            var all = await _companies.FindAllCompanies();
            Assert.AreEqual(created, found);
            Assert.IsTrue(all.Contains(created));
        }

        [Test]
        public async Task TestRemove()
        {
            var company = _companyFiller.FillCompany();
            var created = await _companies.SaveCompany(company);
            var removed = await _companies.RemoveCompany(created.Id);
            Assert.IsTrue(removed);
            var found = await _companies.FindCompanyById(created.Id);
            Assert.IsNull(found);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Task.Yield();
        }
    }
}