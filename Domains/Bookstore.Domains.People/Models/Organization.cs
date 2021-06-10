using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class Organization : Subject, IEquatable<Organization>
    {
        public string OrganizationName { get; set; }
        public override string Name => OrganizationName;
        public override string FullName => OrganizationName;
        public IList<Location> Locations { get; set; }

        public Organization()
        {
            Locations = new List<Location>();
        }

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
            info.AddValue("CompanyName", OrganizationName);
            info.AddValue("Locations", Locations);
        }

        public bool Equals(Organization other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return OrganizationName == other.OrganizationName && Locations.All(other.Locations.Contains) && other.Locations.All(Locations.Contains);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Organization other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OrganizationName, Locations);
        }
    }
}
