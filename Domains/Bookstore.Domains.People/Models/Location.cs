using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class Location : IDomainObject, IEquatable<Location>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public Guid? CompanyId { get; set; }
        public bool Primary { get; set; }
        public Address MailingAddress { get; set; }
        public Address StreetAddress { get; set; }
        public ICollection<Person> Contacts { get; set; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Primary", Primary);
            info.AddValue("MailingAddress", MailingAddress);
            info.AddValue("StreetAddress", StreetAddress);
            info.AddValue("Contacts", Contacts);
        }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var contactsEqual = Contacts != null && other.Contacts != null
                ? Contacts.All(other.Contacts.Contains) && other.Contacts.All(Contacts.Contains)
                : (Contacts?.Count ?? 0) == (other.Contacts?.Count ?? 0);
            return Primary == other.Primary && Equals(MailingAddress, other.MailingAddress) && Equals(StreetAddress, other.StreetAddress) && contactsEqual;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Primary, MailingAddress, StreetAddress, Contacts);
        }
    }
}
