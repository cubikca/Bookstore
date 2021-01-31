using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Commands
{
    public class SaveCountryCommand
    {
        public Country Country { get; set; }
    }
}
