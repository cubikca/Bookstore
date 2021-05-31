using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bookstore.Domains.Book.QueryResults
{
    public class QueryResult<T> : Result where T : new()
    {
        public IList<T> Results { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Results), Results);
        }
    }
}