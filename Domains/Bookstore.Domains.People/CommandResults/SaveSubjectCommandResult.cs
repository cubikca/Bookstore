using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveSubjectCommandResult : CommandResult
    {
        public Subject Subject { get; set; }
    }
}
