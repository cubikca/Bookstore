using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveProvinceCommandResult : CommandResult
    {
        public Province Province { get; set; }
    }
}
