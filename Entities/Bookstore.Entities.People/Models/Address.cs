using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Address : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CountryId { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public bool Deleted { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public virtual Province Province { get; set; }
        public virtual Country Country { get; set; }
        public string PostalCode { get; set; }
   }
}
