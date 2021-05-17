using System;

namespace Bookstore.Domains.Book.Queries
{
    public class FindPublishersQuery
    {
        public Guid? PublisherId { get; set; }
    }
}