using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Bookstore.Entities.People.Models
{
    [Table("Companies")]
    public class Company : Subject
    {
        public string CompanyName { get; set; }
        public virtual List<Location> Locations { get; set; }
        public override string Name => CompanyName;
        public override string FullName => CompanyName;
        public override Address MailingAddress => Locations.FirstOrDefault(l => l.Primary)?.MailingAddress;
        public override Address StreetAddress => Locations.FirstOrDefault(l => l.Primary)?.StreetAddress;
    }
}
