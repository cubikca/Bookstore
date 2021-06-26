using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People
{
    public class Result : IResult, ISerializable
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }
        public string Message { get; set; }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Success), Success);
            info.AddValue(nameof(Status), Status);
            info.AddValue(nameof(Error), Error);
            info.AddValue(nameof(Warning), Warning);
            info.AddValue(nameof(Message), Message);
        }
    }
}