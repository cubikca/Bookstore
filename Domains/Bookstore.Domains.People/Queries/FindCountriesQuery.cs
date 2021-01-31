using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.QueryResults;

namespace Bookstore.Domains.People.Queries
{
    public class FindCountriesQuery
    {
        public Guid? CountryId { get; set; }
    }
}
