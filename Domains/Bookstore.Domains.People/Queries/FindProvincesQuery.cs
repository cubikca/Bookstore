using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.QueryResults;

namespace Bookstore.Domains.People.Queries
{
    public class FindProvincesQuery 
    {
        public string ProvinceAbbreviation { get; set; }
        public string CountryAbbreviation { get; set; }
    }
}
