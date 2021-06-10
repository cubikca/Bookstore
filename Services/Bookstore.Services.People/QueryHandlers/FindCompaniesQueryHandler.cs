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
    public class FindCompaniesQueryHandler : IConsumer<FindCompaniesQuery>
    {
        private readonly IOrganizationRepository _organizations;

        public FindCompaniesQueryHandler(IOrganizationRepository organizations)
        {
            _organizations = organizations;
        }

        public async Task Consume(ConsumeContext<FindCompaniesQuery> context)
        {
            var result = new FindCompaniesQueryResult { Results = new List<Organization>() } ;
            try
            {
                if (context.Message.CompanyId.HasValue)
                {
                    var company = await _organizations.Find(context.Message.CompanyId.Value);
                    if (company != null)
                        result.Results.Add(company);
                }
                else
                {
                    var companies = await _organizations.FindAll();
                    result.Results = companies.ToList();
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
