using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class PersonKnownAsName
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public virtual Person Person { get; set; }
        public string KnownAsName { get; set; }
    }
}
