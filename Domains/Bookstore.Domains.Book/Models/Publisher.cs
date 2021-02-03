using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Publisher : IEquatable<Publisher>
    {
        public Guid Id { get; set; }
        public Subject Details { get; set; }
        public IList<Book> Books { get; set; }

        public bool Equals(Publisher other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
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
            return Id.GetHashCode();
        }
    }
}
