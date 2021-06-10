using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveAddressCommandResult : CommandResult
    {
        public Address Address { get; set; }
    }
}