using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bookstore.Domains.People.Models
{
    public class PhoneNumber : IDomainObject, IEquatable<PhoneNumber>
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string AreaCode { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"({AreaCode}) {Phone}" + (!string.IsNullOrEmpty(Extension) ? $"x{Extension}" : "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("AreaCode", AreaCode);
            info.AddValue("Phone", Phone);
            info.AddValue("Extension", Extension);
        }

        public bool Equals(PhoneNumber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AreaCode == other.AreaCode && Phone == other.Phone && Extension == other.Extension;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PhoneNumber) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, AreaCode, Phone, Extension);
        }
    }
}
