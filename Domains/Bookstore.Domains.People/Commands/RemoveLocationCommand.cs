using System;

namespace Bookstore.Domains.People.Commands
{
    public class RemoveLocationCommand
    {
        public Guid LocationId { get; set; }
    }
}