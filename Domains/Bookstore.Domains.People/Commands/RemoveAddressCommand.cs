using System;

namespace Bookstore.Domains.People.Commands
{
    public class RemoveAddressCommand
    {
        public Guid AddressId { get; set; }
    }
}