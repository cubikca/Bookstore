using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Bookstore.Entities.People.Models
{
    [Table("Companies")]
    public class Company : Subject
    {
        public string CompanyName { get; set; }
        private ICollection<Location> _locations;

        public virtual ICollection<Location> Locations
        {
            get => _locations ??= new List<Location>();
            set => _locations = value;
        }
        public override string Name => CompanyName;
        public override string FullName => CompanyName;
        public override Address MailingAddress => Locations.FirstOrDefault(l => l.Primary)?.MailingAddress;
        public override Address StreetAddress => Locations.FirstOrDefault(l => l.Primary)?.StreetAddress;
    }
}
