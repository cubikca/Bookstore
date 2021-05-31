using System;
using System.Runtime.Serialization;

namespace Bookstore.Domains.Book
{
    public class Result : IResult, ISerializable
    {
        public int Status { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Warning { get; set; }
        public string Error { get; set; }
        public Exception Exception { get; set; }
        
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Status), Status);
            info.AddValue(nameof(Success), Success);
            info.AddValue(nameof(Message), Message);
            info.AddValue(nameof(Warning), Warning);
            info.AddValue(nameof(Error), Error);
            info.AddValue(nameof(Exception), Exception);
        }
    }
}