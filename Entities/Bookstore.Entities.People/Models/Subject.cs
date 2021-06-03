using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    [Table("Subjects")]
    public abstract class Subject : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public virtual Address MailingAddress { get; set; }
        public virtual Address StreetAddress { get; set; }
    }
}
