using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;

namespace Bookstore.Entities.People.Repositories
{
    /* A convenience repository that violates loose coupling principles. Don't go too far down this road. */
    public class SubjectRepository : ISubjectRepository
    {
        private readonly IPersonRepository _people;
        private readonly IOrganizationRepository _organizations;

        public SubjectRepository(IPersonRepository people, IOrganizationRepository organizations)
        {
            _people = people;
            _organizations = organizations;
        }
        
        public async Task<Subject> Save(Subject model)
        {
            if (model is Person person)
                return await _people.Save(person);
            if (model is Organization company)
                return await _organizations.Save(company);
            throw new PeopleException("Cannot save unknown Subject type");
        }

        public async Task<Subject> Find(Guid id)
        {
            var person = await _people.Find(id);
            var company = await _organizations.Find(id);
            return (Subject) person ?? company;
        }

        public async Task<ICollection<Subject>> FindAll()
        {
            var people = (await _people.FindAll()).Cast<Subject>();
            var companies = (await _organizations.FindAll()).Cast<Subject>();
            return people.Union(companies).ToList();
        }

        public async Task<bool> Remove(Guid id)
        {
            var person = await _people.Find(id);
            var company = await _organizations.Find(id);
            var personRemoved = false;
            var companyRemoved = false;
            if (person != null)
                personRemoved = await _people.Remove(id);
            else if (company != null)
                companyRemoved = await _organizations.Remove(id);
            return personRemoved || companyRemoved;
        }
    }
}