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
        // Books by author is found in the navigation property Books of Author
        // Books by publisher is found in the navigation property Books of Publisher
        Task<bool> RemoveBook(Guid bookId);
    }
}
