using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface IPersonRepository
    {
        Task<Person> SavePerson(Person person);
        Task<IList<Person>> FindAllPeople();
        Task<Person> FindPersonById(Guid personId);
        Task<bool> RemovePerson(Guid personId);
    }
}
