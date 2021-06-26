using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using MassTransit.MessageData;
using Newtonsoft.Json;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindCountriesQueryHandler : IConsumer<FindCountriesQuery> 
    {
        private readonly ICountryRepository _countries;
        private readonly IMessageDataRepository _messageData;

        public FindCountriesQueryHandler(ICountryRepository countries, IMessageDataRepository messageData)
        {
            _countries = countries;
            _messageData = messageData;
        }

        public async Task Consume(ConsumeContext<FindCountriesQuery> context)
        {
            var result = new FindCountriesQueryResult();
            try
            {
                var countries = Enumerable.Empty<Country>().ToList();
                if (context.Message.CountryId.HasValue)
                {
                    var country = await _countries.Find(context.Message.CountryId.Value);
                    if (country != null)
                        countries.Add(country);
                }
                else
                    countries = (await _countries.FindAll()).ToList();
                var json = JsonConvert.SerializeObject(countries);
                result.Results = await _messageData.PutString(json);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}
