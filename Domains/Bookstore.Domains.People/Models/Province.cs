using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class Province : IDomainObject, IEquatable<Province>
    {
        public Country Country { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Country", Country);
            info.AddValue("Name", Name);
            info.AddValue("Abbreviation", Abbreviation);
        }

        public bool Equals(Province other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Abbreviation == other.Abbreviation && Equals(Country, other.Country);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Province) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Abbreviation, Country);
        }

        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
