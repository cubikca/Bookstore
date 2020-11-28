using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveCountryCommandResult : CommandResult
    {
        public Country Country { get; set; }
    }
}
