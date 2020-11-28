using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Author
    {
        public Subject Details { get; set; }
        public decimal Salary { get; set; }
    }
}
