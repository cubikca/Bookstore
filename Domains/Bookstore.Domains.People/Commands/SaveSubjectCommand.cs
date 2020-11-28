using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Commands
{
    public class SaveSubjectCommand : Command<SaveSubjectCommandResult>
    {
        public Subject Subject { get; set; }
    }
}
