using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.QueryResults
{
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResult<T> : Result
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects, ItemTypeNameHandling = TypeNameHandling.Objects)]
        public IList<T> Results { get; set; } 
    }
}