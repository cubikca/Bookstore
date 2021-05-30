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

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindCountriesQueryHandler : IConsumer<FindCountriesQuery> 
    {
        private readonly ICountryRepository _countries;

        public FindCountriesQueryHandler(ICountryRepository countries)
        {
            _countries = countries;
        }

        public async Task Consume(ConsumeContext<FindCountriesQuery> context)
        {
            var result = new FindCountriesQueryResult {Results = new List<Country>()};
            try
            {
                if (context.Message.CountryAbbreviation != null)
                {
                    var country = await _countries.FindCountryByAbbreviation(context.Message.CountryAbbreviation);
                    if (country != null)
                        result.Results.Add(country);
                }
                else
                {
                    var countries = await _countries.FindAllCountries();
                    result.Results = countries.ToList();
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
