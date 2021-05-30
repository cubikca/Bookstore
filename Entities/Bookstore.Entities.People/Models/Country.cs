using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Country
    {
        [Key]
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public virtual IList<Province> Provinces { get; set; }
    }
}
