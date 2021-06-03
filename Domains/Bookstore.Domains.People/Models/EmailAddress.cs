using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class EmailAddress : IDomainObject, IEquatable<EmailAddress>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public string Email { get; set; }
        public bool Verified { get; set; }
        public bool Primary { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Email", Email);
            info.AddValue("Verified", Verified);
            info.AddValue("Primary", Primary);
        }

        public bool Equals(EmailAddress other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Email == other.Email && Verified == other.Verified && Primary == other.Primary;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmailAddress) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email, Verified, Primary);
        }
    }
}
