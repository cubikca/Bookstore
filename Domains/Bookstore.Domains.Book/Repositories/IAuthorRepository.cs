using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<ICollection<Author>> FindByBook(Guid bookId);
    }
}
