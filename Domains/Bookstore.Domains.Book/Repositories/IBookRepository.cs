using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IBookRepository : IDisposable
    {
        Task<Models.Book> SaveBook(Models.Book book);
        Task<IList<Models.Book>> FindAllBooks();
        Task<Models.Book> FindBookById(Guid bookId);
        Task<bool> RemoveBook(Guid bookId);
    }
}
