using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.CommandResults;

namespace Bookstore.Domains.People.Commands
{
    public class RemoveCountryCommand 
    {
        public Guid CountryId { get; set; }
    }
}
