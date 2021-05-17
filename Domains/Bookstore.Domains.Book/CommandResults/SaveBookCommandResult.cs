using System.Runtime.Serialization;

namespace Bookstore.Domains.Book.CommandResults
{
    public class SaveBookCommandResult : CommandResult
    {
        public Models.Book Book { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Book), Book);
        }
    }
}