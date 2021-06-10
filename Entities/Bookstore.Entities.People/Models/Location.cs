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
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        public Guid? OrganizationId { get; set; }
        public bool Primary { get; set; }
        public Guid? MailingAddressId { get; set; }
        [ForeignKey("MailingAddressId")]
        public virtual Address MailingAddress { get; set; }
        public Guid? StreetAddressId { get; set; }
        [ForeignKey("StreetAddressId")]
        public virtual Address StreetAddress { get; set; }

        private IList<LocationContact> _contacts;
        public virtual IList<LocationContact> Contacts
        {
            get => _contacts ??= new List<LocationContact>();
            set => _contacts = value;
        }
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }
    }
}
