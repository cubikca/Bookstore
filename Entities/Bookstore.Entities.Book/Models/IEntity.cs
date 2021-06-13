using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Entities.Book.Models
{
    public interface IEntity
    {
        [Key]
        public Guid Id { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}