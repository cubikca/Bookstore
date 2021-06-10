using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Bookstore.Entities.People.Models
{
    [Table("Organizations")]
    public class Organization : Subject
    {
        public string OrganizationName { get; set; }
        public virtual ICollection<Location> Locations { get; set; }

        public override string Name => OrganizationName;
        public override string FullName => OrganizationName;
        public override Address MailingAddress
        {
            get
            {
                return Locations.FirstOrDefault(l => l.Primary)?.MailingAddress;
            }
            set
            {
                var location = Locations.FirstOrDefault(l => l.Primary);
                if (location != null)
                    location.MailingAddress = value;
            }
        }

        public override Address StreetAddress
        {
            get
            {
                return Locations.FirstOrDefault(l => l.Primary)?.StreetAddress;
            }
            set
            {
                var location = Locations.FirstOrDefault(l => l.Primary);
                if (location != null)
                    location.StreetAddress = value;
            }
        }
    }
}
