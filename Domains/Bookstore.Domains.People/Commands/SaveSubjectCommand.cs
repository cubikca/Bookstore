using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Models;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Commands
{
    public class SaveSubjectCommand 
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public Subject Subject { get; set; }
    }
}
