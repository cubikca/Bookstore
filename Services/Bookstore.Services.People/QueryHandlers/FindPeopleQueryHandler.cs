using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindPeopleQueryHandler : QueryHandlerBase<FindPeopleQuery, FindPeopleQueryResult, Person>
    {
        private readonly IPersonRepository _people;

        public FindPeopleQueryHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, IPersonRepository people) : base(connection, mqOptions)
        {
            _people = people;
        }

        public override async Task<FindPeopleQueryResult> Handle(FindPeopleQuery request, CancellationToken cancellationToken)
        {
            var result = new FindPeopleQueryResult {CorrelationId = request.Id, Results = new List<Person>()};
            try
            {
                if (request.PersonId.HasValue)
                {
                    var person = await _people.FindPersonById(request.PersonId.Value);
                    if (person != null)
                        result.Results.Add(person);
                }
                else
                {
                    var people = await _people.FindAllPeople();
                    result.Results = people;
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = ex;
            }
            return result;
        }
    }
}
