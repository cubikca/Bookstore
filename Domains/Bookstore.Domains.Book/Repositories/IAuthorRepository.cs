using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IAuthorRepository
    {
        Task<Author> SaveAuthor(Author author); 
        Task<IList<Author>> FindAllAuthors();
        Task<Author> FindAuthorById(Guid authorId);
        Task<bool> RemoveAuthor(Guid authorId); // and all books by that author
    }
}
