using System;

namespace Bookstore.Domains.Book.Queries
{
    public class FindAuthorsQuery
    {
        public Guid? AuthorId { get; set; }
        public Guid? BookId { get; set; }
    }
}