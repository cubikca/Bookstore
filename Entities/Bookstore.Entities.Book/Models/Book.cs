using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Entities.Book.Models
{
    public class Book : IEntity, IEquatable<Book>
    {
        [Key]
        public Guid Id { get; set; }

        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public virtual IList<Author> Authors { get; set; }
        public int Edition { get; set; }
        public DateTime PublishDate { get; set; }
        public Guid? PublisherId { get; set; }
        [ForeignKey("PublisherId")]
        public virtual Publisher Publisher { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }

        public bool Equals(Book other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && ISBN == other.ISBN && Title == other.Title && Subtitle == other.Subtitle && Edition == other.Edition && PublishDate.Equals(other.PublishDate) && Cost == other.Cost && Price == other.Price;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Book) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ISBN, Title, Subtitle, Edition, PublishDate, Cost, Price);
        }
    }
}
