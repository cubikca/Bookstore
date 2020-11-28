using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Commands
{
    public class SaveCountryCommand : Command<SaveCountryCommandResult>
    {
        public Country Country { get; set; }
    }
}
