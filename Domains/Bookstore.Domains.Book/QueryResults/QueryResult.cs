using System.Collections.Generic;
using System.Runtime.Serialization;
using MassTransit;
using Newtonsoft.Json;

namespace Bookstore.Domains.Book.QueryResults
{
    public class QueryResult<T> : Result where T : class, new()
    {
        // these results can exceed the message size limit in some brokers. we will encode the results
        // as a BSON string and store in Azure blob. The API will take care of wrapping and unwrapping these results.
        public MessageData<string> Results { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Results), Results);
        }
    }
}