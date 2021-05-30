using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Bookstore.Entities.People.Models
{
    public class Province
    {
        public string Abbreviation { get; set; }
        public Guid CountryId { get; }
        public string Name { get; set; }

        public Province(string abbreviation, Guid countryId)
        {
            Abbreviation = abbreviation;
            CountryId = countryId;
        }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
    }
}
