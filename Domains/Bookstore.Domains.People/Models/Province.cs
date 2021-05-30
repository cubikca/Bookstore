using System;
using System.Runtime.Serialization;

namespace Bookstore.Domains.People.Models
{
    public class Province : ISerializable, IEquatable<Province>
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
            return HashCode.Combine(Name, Abbreviation, Country);
        }
    }
}
