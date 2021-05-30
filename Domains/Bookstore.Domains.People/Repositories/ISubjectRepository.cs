using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface ISubjectRepository
    {
        Task<Subject> SaveSubject(Subject subject);
        Task<ICollection<Subject>> FindAllSubjects();
        Task<Subject> FindSubjectById(Guid subjectId);
        Task<bool> RemoveSubject(Guid subjectId);
    }
}
