using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveLocationCommandResult : CommandResult
    {
        public Location Location { get; set; }
    }
}