using System;
using System.Runtime.Serialization;

namespace Bookstore.Domains.Book.Commands
{
    public class RemoveAuthorCommand : ISerializable
    {
        public Guid AuthorId { get; set; }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(AuthorId), AuthorId);
        }
    }
}