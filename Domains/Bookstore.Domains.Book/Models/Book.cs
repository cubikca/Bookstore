using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Book : IDomainObject, IEquatable<Book>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public IList<Author> Authors { get; set; }
        public int Edition { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public Publisher Publisher { get; set; }
        public Guid? PublisherId { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }

        public bool Equals(Book other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ISBN == other.ISBN && Title == other.Title && Subtitle == other.Subtitle && Edition == other.Edition && PublishDate.Equals(other.PublishDate) && Cost == other.Cost && Price == other.Price
                && Authors.All(other.Authors.Contains) && other.Authors.All(Authors.Contains)
                && Publisher.Equals(other.Publisher);
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

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(ISBN), ISBN);
            info.AddValue(nameof(Title), Title);
            info.AddValue(nameof(Subtitle), Subtitle);
            info.AddValue(nameof(Edition), Edition);
            info.AddValue(nameof(PublishDate), PublishDate);
            info.AddValue(nameof(Cost), Cost);
            info.AddValue(nameof(Price), Price);
            info.AddValue(nameof(Authors), Authors);
            info.AddValue(nameof(Publisher), Publisher);
        }
    }
}
