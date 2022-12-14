using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class Country : IDomainObject, IEquatable<Country>
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Abbreviation", Abbreviation);
        }

        public bool Equals(Country other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Abbreviation == other.Abbreviation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Country) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Abbreviation);
        }

        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
