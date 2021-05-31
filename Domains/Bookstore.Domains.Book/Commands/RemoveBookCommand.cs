using System;
using System.Runtime.Serialization;

namespace Bookstore.Domains.Book.Commands
{
    public class RemoveBookCommand : ISerializable
    {
        public Guid BookId { get; set; }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(BookId), BookId);
        }
    }
}