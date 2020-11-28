using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.QueryResults;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Queries
{
    public class FindCompaniesQuery : Query<FindCompaniesQueryResult, Company>
    {
        public Guid? CompanyId { get; set; }
    }
}
