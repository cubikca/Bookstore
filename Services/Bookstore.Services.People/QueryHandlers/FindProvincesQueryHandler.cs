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
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindProvincesQueryHandler : QueryHandlerBase<FindProvincesQuery, FindProvincesQueryResult, Province>
    {
        private readonly ICountryRepository _countries;

        public FindProvincesQueryHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<FindProvincesQueryResult> Handle(FindProvincesQuery request, CancellationToken cancellationToken)
        {
            var queryResults = new List<Province>();
            var result = new FindProvincesQueryResult {CorrelationId = request.Id, Results = queryResults};
            try
            {
                if (request.ProvinceId.HasValue && request.CountryId.HasValue) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (!request.ProvinceId.HasValue && !request.CountryId.HasValue) throw new PeopleException("Must provide exactly one of province and country id for retrieve provinces query");
                if (request.ProvinceId.HasValue)
                {
                    var province = await _countries.FindProvinceById(request.ProvinceId.Value);
                    if (province != null) queryResults.Add(province);
                }
                if (request.CountryId.HasValue)
                {
                    var provinces = await _countries.FindProvincesByCountryId(request.CountryId.Value);
                    queryResults.AddRange(provinces ?? Enumerable.Empty<Province>());
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
