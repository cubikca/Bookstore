using System;
using System.Runtime.Serialization;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Author : IDomainObject, IEquatable<Author>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public Subject Profile { get; set; }
        public decimal Salary { get; set; }

        public bool Equals(Author other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Profile?.Id == other.Profile?.Id && Salary == other.Salary;
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
            return HashCode.Combine(Id, Profile, Salary);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(Profile), Profile);
            info.AddValue(nameof(Salary), Salary);
            info.AddValue(nameof(Created), Created);
            info.AddValue(nameof(CreatedBy), CreatedBy);
            info.AddValue(nameof(Updated), Updated);
            info.AddValue(nameof(UpdatedBy), UpdatedBy);
        }
    }
}
