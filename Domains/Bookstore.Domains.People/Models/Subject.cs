using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    /*
     * We want to have an abstract concept that applies to both people and companies.
     * Many systems allow both people and companies to fill various roles (client, customer, supplier, etc.)
     * We don't want to treat people and companies differently in cases where they fill the same role
     */
    public abstract class Subject : IDomainObject
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract Address StreetAddress { get; set; }
        public abstract Address MailingAddress { get; set; }
        public EmailAddress EmailAddress { get; set; }
        public PhoneNumber PhoneNumber { get; set; }
        public OnlinePresence OnlinePresence { get; set; }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Address", StreetAddress);
            info.AddValue("MailingAddress", MailingAddress);
            info.AddValue("EmailAddress", EmailAddress);
            info.AddValue("PhoneNumber", PhoneNumber);
            info.AddValue("OnlinePresence", OnlinePresence);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, FullName, StreetAddress, MailingAddress, EmailAddress, PhoneNumber, OnlinePresence);
        }
    }
}
