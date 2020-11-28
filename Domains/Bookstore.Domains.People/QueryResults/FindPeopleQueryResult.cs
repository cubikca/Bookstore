using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.People.Models;
using RabbitWarren.Messaging;

namespace Bookstore.Domains.People.QueryResults
{
    public class FindPeopleQueryResult : QueryResult<Person>
    {
    }
}
