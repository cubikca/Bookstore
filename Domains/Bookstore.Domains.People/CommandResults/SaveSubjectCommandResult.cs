using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Bookstore.Domains.People.Models;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.CommandResults
{
    public class SaveSubjectCommandResult : CommandResult
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public Subject Subject { get; set; }
    }
}
