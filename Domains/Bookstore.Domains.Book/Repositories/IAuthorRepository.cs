using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IAuthorRepository
    {
        Author SaveAuthor(Author author);
        IList<Author> FindAllAuthors();
        Author FindAuthorById(Guid authorId);
        bool RemoveAuthor(Guid authorId);
    }
}
