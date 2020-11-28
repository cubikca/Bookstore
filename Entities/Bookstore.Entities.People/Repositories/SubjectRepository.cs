using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public SubjectRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<SubjectRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
        }

       public async Task<IList<Subject>> FindAllSubjects()
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
    }
}
