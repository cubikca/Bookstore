using System;
using System.Linq;
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
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Entities.People
{
    [TestFixture]
    public class CompanyTests
    {
        private ICompanyRepository _companies;
        private IMapper _mapper;
        private CompanyFiller _companyFiller;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer(
                    "Data Source=(local);Initial Catalog=PeopleCompanyTests;User Id=brian;Password=development");
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
            var people = new PersonRepository(sp.GetService<IDbContextFactory<PeopleContext>>(), _mapper, sp.GetService<ILogger<PersonRepository>>());
            _companies = new CompanyRepository(sp.GetService<IDbContextFactory<PeopleContext>>(), _mapper, sp.GetService<ILogger<CompanyRepository>>(), people);
            _companyFiller = new CompanyFiller();
            await using var db = dbFactory.CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        [Test]
        public async Task TestSave()
        {
            var company = _companyFiller.FillCompany();
            var created = await _companies.SaveCompany(company);
            Assert.AreNotSame(company, created);
            Assert.AreEqual(company, created);
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