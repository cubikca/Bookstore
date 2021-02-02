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
    public class FindProvincesQueryHandler : IConsumer<FindProvincesQuery>
    {
        private readonly ICountryRepository _countries;

        public FindProvincesQueryHandler(ICountryRepository countries)
        {
            _countries = countries;
        }

        public async Task Consume(ConsumeContext<FindProvincesQuery> context)
        {
            var queryResults = new List<Province>();
            var result = new FindProvincesQueryResult {Results = queryResults};
            try
            {
                if (context.Message.ProvinceId.HasValue && context.Message.CountryId.HasValue) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (!context.Message.ProvinceId.HasValue && !context.Message.CountryId.HasValue) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (context.Message.ProvinceId.HasValue)
                {
                    var province = await _countries.FindProvinceById(context.Message.ProvinceId.Value);
                    if (province != null) queryResults.Add(province);
                }
                if (context.Message.CountryId.HasValue)
                {
                    var provinces = await _countries.FindProvincesByCountryId(context.Message.CountryId.Value);
                    queryResults.AddRange(provinces ?? Enumerable.Empty<Province>());
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
