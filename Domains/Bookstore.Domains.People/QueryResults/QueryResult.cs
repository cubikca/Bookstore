using System.Collections.Generic;

namespace Bookstore.Domains.People.QueryResults
{
    public class QueryResult<T> : Result
    {
        public IList<T> Results { get; set; } 
    }
}