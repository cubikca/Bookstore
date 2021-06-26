using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Entities.People.Models
{
    public interface IEntity
    {
        [Key]
        public Guid Id { get; set; } 
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
    }
}