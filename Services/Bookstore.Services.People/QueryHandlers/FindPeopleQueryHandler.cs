using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using MassTransit;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindPeopleQueryHandler : IConsumer<FindPeopleQuery>
    {
        private readonly IPersonRepository _people;

        public FindPeopleQueryHandler(IPersonRepository people)
        {
            _people = people;
        }

        public async Task Consume(ConsumeContext<FindPeopleQuery> context)
        {
            var result = new FindPeopleQueryResult {Results = new List<Person>()};
            try
            {
                if (context.Message.PersonId.HasValue)
                {
                    var person = await _people.FindPersonById(context.Message.PersonId.Value);
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
            await context.RespondAsync(result);
        }
    }
}
