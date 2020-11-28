using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bookstore.Domains.People.Models
{
    /*
     * We want to have an abstract concept that applies to both people and companies.
     * Many systems allow both people and companies to fill various roles (client, customer, supplier, etc.)
     * We don't want to treat people and companies differently in cases where they fill the same role
     */
    public class Subject : ISerializable, IEquatable<Subject>
    {
        public Guid Id { get; set; }
        public virtual string Name { get; protected set; }
        public virtual string FullName { get; protected set; }
        public virtual Address StreetAddress { get; set; }
        public virtual Address MailingAddress { get; set; }
        public virtual EmailAddress EmailAddress { get; set; }
        public virtual PhoneNumber PhoneNumber { get; set; }
        public virtual OnlinePresence OnlinePresence { get; set; }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Address", StreetAddress);
            info.AddValue("MailingAddress", MailingAddress);
            info.AddValue("EmailAddress", EmailAddress);
            info.AddValue("PhoneNumber", PhoneNumber);
            info.AddValue("OnlinePresence", OnlinePresence);
        }

        public bool Equals(Subject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Name == other.Name && FullName == other.FullName && Equals(StreetAddress, other.StreetAddress) && Equals(MailingAddress, other.MailingAddress) && Equals(EmailAddress, other.EmailAddress) && Equals(PhoneNumber, other.PhoneNumber) && Equals(OnlinePresence, other.OnlinePresence);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subject) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, FullName, StreetAddress, MailingAddress, EmailAddress, PhoneNumber, OnlinePresence);
        }
    }
}
