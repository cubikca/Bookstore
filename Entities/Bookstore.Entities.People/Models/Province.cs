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
        public string CountryAbbreviation { get; set; }
        public string Name { get; set; }

        public Province(string abbreviation, string countryAbbreviation)
        {
            Abbreviation = abbreviation;
            CountryAbbreviation = countryAbbreviation;
        }

        [ForeignKey("CountryAbbreviation")]
        public virtual Country Country { get; set; }
    }
}
