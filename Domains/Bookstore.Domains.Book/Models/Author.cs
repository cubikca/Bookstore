using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public Subject Details { get; set; }
        public decimal Salary { get; set; }
        public virtual IList<Book> Books { get; set; }
    }
}
