using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class LocationContact
    {
        public Guid LocationId { get; set; }
        public Guid ContactId { get; set; }
        public virtual Location Location { get; set; }
        public virtual Person Contact { get; set; }
    }
}
