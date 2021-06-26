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
using MassTransit.MessageData;
using Newtonsoft.Json;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindPeopleQueryHandler : IConsumer<FindPeopleQuery>
    {
        private readonly IPersonRepository _people;
        private readonly IMessageDataRepository _messageData;

        public FindPeopleQueryHandler(IPersonRepository people, IMessageDataRepository messageData)
        {
            _people = people;
            _messageData = messageData;
        }

        public async Task Consume(ConsumeContext<FindPeopleQuery> context)
        {
            var result = new FindPeopleQueryResult();
            try
            {
                var people = Enumerable.Empty<Person>().ToList();
                if (context.Message.PersonId.HasValue)
                {
                    var person = await _people.Find(context.Message.PersonId.Value);
                    if (person != null)
                        people.Add(person);
                }
                else
                    people = (await _people.FindAll()).ToList();
                var json = JsonConvert.SerializeObject(people);
                result.Results = await _messageData.PutString(json);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}
