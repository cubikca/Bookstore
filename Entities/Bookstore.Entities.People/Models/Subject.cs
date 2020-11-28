using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    [Table("Subjects")]
    public abstract class Subject
    {
        [Key]
        public Guid Id { get; set; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public virtual Address MailingAddress { get; set; }
        public virtual Address StreetAddress { get; set; }
        public virtual EmailAddress EmailAddress { get; set; }
        public virtual OnlinePresence OnlinePresence { get; set; }
        public virtual PhoneNumber PhoneNumber { get; set; }
    }
}
