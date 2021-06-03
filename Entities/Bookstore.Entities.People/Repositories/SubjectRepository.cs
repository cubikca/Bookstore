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
        private readonly ICompanyRepository _companies;

        public SubjectRepository(IPersonRepository people, ICompanyRepository companies)
        {
            _people = people;
            _companies = companies;
        }
        
        public async Task<Subject> Save(Subject model)
        {
            if (model is Person person)
                return await _people.Save(person);
            if (model is Company company)
                return await _companies.Save(company);
            throw new PeopleException("Cannot save unknown Subject type");
        }

        public async Task<Subject> Find(Guid id)
        {
            var person = await _people.Find(id);
            var company = await _companies.Find(id);
            return (Subject) person ?? company;
        }

        public async Task<ICollection<Subject>> FindAll()
        {
            var people = (await _people.FindAll()).Cast<Subject>();
            var companies = (await _companies.FindAll()).Cast<Subject>();
            return people.Union(companies).ToList();
        }

        public async Task<bool> Remove(Guid id)
        {
            var person = await _people.Find(id);
            var company = await _companies.Find(id);
            var personRemoved = false;
            var companyRemoved = false;
            if (person != null)
                personRemoved = await _people.Remove(id);
            else if (company != null)
                companyRemoved = await _companies.Remove(id);
            return personRemoved || companyRemoved;
        }
    }
}