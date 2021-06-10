using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Commands
{
    public class SaveLocationCommand
    {
        public Location Location { get; set; }
    }
}