using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IBookRepository : IRepository<Bookstore.Domains.Book.Models.Book>
    {
    }
}
