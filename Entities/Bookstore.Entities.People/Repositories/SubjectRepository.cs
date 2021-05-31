using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.People.Repositories
{
    public class SubjectRepository : RepositoryBase, ISubjectRepository
    {
        private readonly ILogger<SubjectRepository> _logger;
        private readonly IPersonRepository _people;
        private readonly ICompanyRepository _companies;

        public SubjectRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IPersonRepository people, ICompanyRepository companies, ILogger<SubjectRepository> logger) : base(dbFactory, mapper)
        {
            _people = people;
            _companies = companies;
            _logger = logger;
        }

        public async Task<Subject> SaveSubject(Subject subject)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var result = subject switch
                {
                    Person person => (Subject) await _people.SavePerson(person),
                    Company company => await _companies.SaveCompany(company),
                    _ => throw new PeopleException("Unknown subject type")
                };
                return result;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unable to save Subject");
                 throw new PeopleException("Unable to save Subject", ex);
            }
        }

        public async Task<ICollection<Subject>> FindAllSubjects()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.Subjects.ToListAsync();
                var subjects = new List<Subject>();
                foreach (var entity in entities)
                {
                    Subject subject = entity switch
                    {
                        Models.Person person => MapPerson(person),
                        Models.Company company => MapCompany(company),
                        _ => null
                    };
                    if (subject != null)
                        subjects.Add(subject);
                }
                return subjects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve subject data");
                throw new PeopleException("Unable to retrieve subject data", ex);
            }
        }

        public async Task<Subject> FindSubjectById(Guid subjectId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Subjects.SingleOrDefaultAsync(s => s.Id == subjectId);
                if (entity == null) return null;
                Subject subject = entity switch
                {
                    Models.Person person => MapPerson(person),
                    Models.Company company => MapCompany(company),
                    _ => null
                };
                return subject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve subject data");
                throw new PeopleException("Unable to retrieve subject data", ex);
            }
        }

        public async Task<bool> RemoveSubject(Guid subjectId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Subjects.SingleOrDefaultAsync(s => s.Id == subjectId);
                var result = entity switch
                {
                    Models.Person person => await _people.RemovePerson(person.Id),
                    Models.Company company => await _companies.RemoveCompany(company.Id),
                    _ => false
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove subject");
                throw new PeopleException("Unable to remove subject", ex);
            }
        }
    }
}
