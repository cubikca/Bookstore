using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
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
        
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}
