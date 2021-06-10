using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Bookstore.Domains.People.Models;

namespace Bookstore.Entities.Book.Models
{
    public class Author : IEntity
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
        public decimal Salary { get; set; }

   }
}
