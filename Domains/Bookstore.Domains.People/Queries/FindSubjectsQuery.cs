using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.QueryResults;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.Queries
{
    public class FindSubjectsQuery : Query<FindSubjectsQueryResult, Subject>
    {
        public Guid? SubjectId { get; set; }
    }
}
