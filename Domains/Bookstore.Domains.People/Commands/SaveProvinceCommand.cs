using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Commands
{
    public class SaveProvinceCommand : Command<SaveProvinceCommandResult>
    {
        public Province Province { get; set; }
    }
}
