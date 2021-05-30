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
        private readonly IProvinceRepository _provinces;

        public FindProvincesQueryHandler(IProvinceRepository provinces)
        {
            _provinces = provinces;
        }

        public async Task Consume(ConsumeContext<FindProvincesQuery> context)
        {
            var queryResults = new List<Province>();
            var result = new FindProvincesQueryResult {Results = queryResults};
            try
            {
                if (context.Message.ProvinceAbbreviation != null && context.Message.CountryAbbreviation != null) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (context.Message.ProvinceAbbreviation == null && context.Message.CountryAbbreviation == null) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (context.Message.ProvinceAbbreviation != null)
                {
                    var province = await _provinces.FindProvinceByAbbreviation(context.Message.ProvinceAbbreviation);
                    if (province != null) queryResults.Add(province);
                }
                if (context.Message.CountryAbbreviation != null)
                {
                    var provinces = await _provinces.FindProvincesByCountryAbbreviation(context.Message.CountryAbbreviation);
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
