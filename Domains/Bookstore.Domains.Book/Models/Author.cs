using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Author : IEquatable<Author>
    {
        public Guid Id { get; set; }
        public Subject Details { get; set; }
        public decimal Salary { get; set; }
        public virtual IList<Book> Books { get; set; }

        public bool Equals(Author other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Salary == other.Salary;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Author) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Details, Salary);
        }
    }
}
