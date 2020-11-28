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
    public class FindCompaniesQueryHandler : QueryHandlerBase<FindCompaniesQuery, FindCompaniesQueryResult, Company>
    {
        private readonly ICompanyRepository _companies;

        public FindCompaniesQueryHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICompanyRepository companies) : base(connection, mqOptions)
        {
            _companies = companies;
        }

        public override async Task<FindCompaniesQueryResult> Handle(FindCompaniesQuery request, CancellationToken cancellationToken)
        {
            var result = new FindCompaniesQueryResult {CorrelationId = request.Id, Results = new List<Company>()};
            try
            {
                if (request.CompanyId.HasValue)
                {
                    var company = await _companies.FindCompanyById(request.CompanyId.Value);
                    if (company != null)
                        result.Results.Add(company);
                }
                else
                {
                    var companies = await _companies.FindAllCompanies();
                    result.Results = companies;
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
