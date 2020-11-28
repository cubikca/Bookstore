using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.CommandResults;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Commands
{
    public class RemoveProvinceCommand : Command<RemoveProvinceCommandResult>
    {
        public Guid ProvinceId { get; set; }
    }
}
