using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Bookstore.Domains.People.Models
{
    public class Company : Subject, IEquatable<Company>
    {
        public string CompanyName { get; set; }
        public override string Name => CompanyName;
        public override string FullName => CompanyName;
        public List<Location> Locations { get; set; }

        public override Address MailingAddress
        {
            get
            {
                return Locations?.FirstOrDefault(l => l.Primary)?.MailingAddress;
            }
        }

        public override Address StreetAddress
        {
            get
            {
                return Locations?.FirstOrDefault(l => l.Primary)?.StreetAddress;
            }
        }

        public Person Contact
        {
            get
            {
                return Locations.Where(l => l.Primary).SelectMany(l => l.Contacts).FirstOrDefault();
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CompanyName", CompanyName);
            info.AddValue("Locations", Locations);
        }

        public bool Equals(Company other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CompanyName == other.CompanyName && Locations.All(other.Locations.Contains) && other.Locations.All(Locations.Contains);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Company other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CompanyName, Locations);
        }
    }
}
