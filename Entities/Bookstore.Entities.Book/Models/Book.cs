using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Entities.Book.Models
{
    public class Book : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public virtual IList<Author> Authors { get; set; }
        public int Edition { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public Guid? PublisherId { get; set; }
        public virtual Publisher Publisher { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ISBN, Title, Subtitle, Edition, PublishDate, Cost, Price);
        }
    }
}
