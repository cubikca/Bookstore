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

namespace Bookstore.Entities.People.Tests
{
    [TestFixture]
    public class SubjectTests
    {
        private ISubjectRepository _subjects;
        // This is only for testing purposes; there is very loose coupling between Subjects and their corresponding subtypes
        private IPersonRepository _people;
        private ICompanyRepository _companies;
        private IAddressRepository _addresses;
        private IMapper _mapper;
        private CompanyFiller _companyFiller;
        private PersonFiller _personFiller;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = "server=localhost;user=brian;password=development;database=PeopleEntitiesTests";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddLogging(opt => opt.AddConsole());
            var sp = services.BuildServiceProvider();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            await using var db = dbFactory.CreateDbContext();
            Assert.NotNull(db);
            _addresses = new AddressRepository(dbFactory, _mapper, sp.GetService<ILogger<AddressRepository>>());
            _people = new PersonRepository(dbFactory, _mapper, _addresses, sp.GetService<ILogger<PersonRepository>>());
            _companies = new CompanyRepository(dbFactory, _mapper, _addresses, _people, sp.GetService<ILogger<CompanyRepository>>());
            _subjects = new SubjectRepository(dbFactory, _mapper, _people, _companies, sp.GetService<ILogger<SubjectRepository>>());
            _companyFiller = new CompanyFiller();
            _personFiller = new PersonFiller();
        }

        [Test]
        public async Task TestFind()
        {
            var company = _companyFiller.FillCompany();
            var person = _personFiller.FillPerson();
            company = await _companies.SaveCompany(company);
            person = await _people.SavePerson(person);
            var all = await _subjects.FindAllSubjects();
            Assert.IsTrue(all.Contains(company));
            Assert.IsTrue(all.Contains(person));
            var subject1 = await _subjects.FindSubjectById(company.Id);
            var subject2 = await _subjects.FindSubjectById(person.Id);
            Assert.AreNotSame(company, subject1);
            Assert.AreNotSame(person, subject2);
            Assert.AreEqual(company, subject1);
            Assert.AreEqual(person, subject2);
        }
    }
}
