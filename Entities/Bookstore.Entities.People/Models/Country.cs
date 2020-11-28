using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Country
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public virtual IList<Province> Provinces { get; set; }
    }
}
