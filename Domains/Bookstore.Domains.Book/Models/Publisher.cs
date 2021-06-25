using System;
using System.Runtime.Serialization;
using Bookstore.Domains.People.Models;
using Newtonsoft.Json;

namespace Bookstore.Domains.Book.Models
{
    public class Publisher : IDomainObject, IEquatable<Publisher>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public Subject Profile { get; set; }
        public Guid? ProfileId { get; set; }

        public bool Equals(Publisher other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Profile.Equals(other.Profile);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Publisher) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Profile);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(Profile), Profile);
        }
    }
}
