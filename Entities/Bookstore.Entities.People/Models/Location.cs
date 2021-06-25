using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Location : IEntity
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
        public Guid? OrganizationId { get; set; }
        public bool Primary { get; set; }
        public Guid? MailingAddressId { get; set; }
        public virtual Address MailingAddress { get; set; }
        public Guid? StreetAddressId { get; set; }
        public virtual Address StreetAddress { get; set; }

        public virtual ICollection<LocationContact> Contacts { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
