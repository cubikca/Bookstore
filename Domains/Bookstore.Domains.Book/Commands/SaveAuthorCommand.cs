using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Commands
{
    public class SaveAuthorCommand
    {
        public Author Author { get; set; }
    }
}