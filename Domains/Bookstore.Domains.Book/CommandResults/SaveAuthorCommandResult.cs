using System.Runtime.Serialization;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.CommandResults
{
    public class SaveAuthorCommandResult : CommandResult
    {
        public Author Author { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Author), Author);
        }
    }
}