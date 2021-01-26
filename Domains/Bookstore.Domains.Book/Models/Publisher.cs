using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Publisher
    {
        public Guid Id { get; set; }
        public Subject Details { get; set; }
        public IList<Book> Books { get; set; }
    }
}
