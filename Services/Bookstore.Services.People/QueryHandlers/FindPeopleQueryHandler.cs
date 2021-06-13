using System;
using System.Collections.Generic;
using System.Linq;
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
                    var person = await _people.Find(context.Message.PersonId.Value);
                    if (person != null)
                        result.Results.Add(person);
                }
                else
                {
                    var people = await _people.FindAll();
                    result.Results = people.ToList();
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
