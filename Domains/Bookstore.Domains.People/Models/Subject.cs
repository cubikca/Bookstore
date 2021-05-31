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
    public abstract class Subject
    {
        public Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string FullName { get; set; }
        public virtual Address StreetAddress { get; set; }
        public virtual Address MailingAddress { get; set; }
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
