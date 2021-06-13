using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Bookstore.Domains.People.Repositories
{
    public interface IRepository<T> where T : ISerializable
    {
        Task<T> Save(T model);
        Task<T> Find(Guid id);
        Task<ICollection<T>> FindAll();
        Task<bool> Remove(Guid id);
    }
}