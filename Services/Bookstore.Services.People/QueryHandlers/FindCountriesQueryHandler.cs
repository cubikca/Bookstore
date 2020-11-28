using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindCountriesQueryHandler : QueryHandlerBase<FindCountriesQuery, FindCountriesQueryResult, Country>
    {
        private readonly ICountryRepository _countries;

        public FindCountriesQueryHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<FindCountriesQueryResult> Handle(FindCountriesQuery request, CancellationToken cancellationToken)
        {
            var result = new FindCountriesQueryResult {CorrelationId = request.Id, Results = new List<Country>()};
            try
            {
                if (request.CountryId.HasValue)
                {
                    var country = await _countries.FindCountryById(request.CountryId.Value);
                    if (country != null)
                        result.Results.Add(country);
                }
                else
                {
                    var countries = await _countries.FindAllCountries();
                    result.Results = countries;
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
