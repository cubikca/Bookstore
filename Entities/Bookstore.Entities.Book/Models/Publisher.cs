using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Entities.Book.Models
{
    public class Publisher : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        public Guid ProfileId { get; set; }
        public virtual IList<Book> Books { get; set; }


        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ProfileId);
        }
    }
}