using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class OnlinePresence : IDomainObject, IEquatable<OnlinePresence>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string LinkedIn { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Website", Website);
            info.AddValue("Facebook", Facebook);
            info.AddValue("Twitter", Twitter);
            info.AddValue("Instagram", Instagram);
            info.AddValue("LinkedIn", LinkedIn);
        }

        public bool Equals(OnlinePresence other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Website == other.Website && Facebook == other.Facebook && Twitter == other.Twitter && Instagram == other.Instagram && LinkedIn == other.LinkedIn;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OnlinePresence) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Website, Facebook, Twitter, Instagram, LinkedIn);
        }
    }
}
