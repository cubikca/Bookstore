using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Commands
{
    public class SavePublisherCommand
    {
        public Publisher Publisher { get; set; }
    }
}