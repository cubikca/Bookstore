using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Entities.Book.Models
{
    public class Publisher : IEquatable<Publisher>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public virtual IList<Book> Books { get; set; }

        public bool Equals(Publisher other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && ProfileId.Equals(other.ProfileId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Publisher) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ProfileId);
        }
    }
}