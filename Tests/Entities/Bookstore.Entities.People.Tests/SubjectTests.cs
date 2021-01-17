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
        private IMapper _mapper;
        private ILogger<PersonRepository> _personLogger;
        private ILogger<CompanyRepository> _companyLogger;
        private ILogger<SubjectRepository> _subjectLogger;
        private CompanyFiller _companyFiller;
        private PersonFiller _personFiller;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer("Data Source=(local);Initial Catalog=PeopleSubjectTests;User Id=brian;Password=development");
            });
            services.AddLogging(opt => opt.AddConsole());
            var sp = services.BuildServiceProvider();
            _personLogger = sp.GetService<ILogger<PersonRepository>>();
            _companyLogger = sp.GetService<ILogger<CompanyRepository>>();
            _subjectLogger = sp.GetService<ILogger<SubjectRepository>>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
            var dbFactory = sp.GetService<IDbContextFactory<PeopleContext>>();
            Assert.NotNull(dbFactory);
            await using var db = dbFactory.CreateDbContext();
            Assert.NotNull(db);
            _people = new PersonRepository(dbFactory, _mapper, _personLogger);
            _companies = new CompanyRepository(dbFactory, _mapper, _companyLogger, _people);
            _subjects = new SubjectRepository(dbFactory, _mapper, _subjectLogger);
            _companyFiller = new CompanyFiller();
            _personFiller = new PersonFiller();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
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
