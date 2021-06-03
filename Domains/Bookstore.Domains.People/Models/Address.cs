using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class Address : IDomainObject, IEquatable<Address>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime Updated { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public Province Province { get; set; }
        public Country Country { get; set; }
        public string PostalCode { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Street1", Street1);
            info.AddValue("Street2", Street2);
            info.AddValue("City", City);
            info.AddValue("Province", Province);
            info.AddValue("Country", Country);
            info.AddValue("PostalCode", PostalCode);
        }

        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Street1 == other.Street1 && Street2 == other.Street2 && City == other.City && PostalCode == other.PostalCode && Equals(Province, other.Province) && Equals(Country, other.Country);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Address other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Street1, Street2, City, Province, Country, PostalCode);
        }
    }
}
