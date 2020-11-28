using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.QueryResults;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Queries
{
    public class FindProvincesQuery : Query<FindProvincesQueryResult, Province>
    {
        public Guid? ProvinceId { get; set; }
        public Guid? CountryId { get; set; }
    }
}
