using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public virtual Province Province { get; set; }
        public virtual Country Country { get; set; }
        public string PostalCode { get; set; }
   }
}
