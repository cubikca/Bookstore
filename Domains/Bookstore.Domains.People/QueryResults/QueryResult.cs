using System.Collections.Generic;
using MassTransit;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.QueryResults
{
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResult<T> : Result
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects, ItemTypeNameHandling = TypeNameHandling.Objects)]
        public MessageData<string> Results { get; set; } 
    }
}