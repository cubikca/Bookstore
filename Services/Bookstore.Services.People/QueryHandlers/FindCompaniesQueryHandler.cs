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
        private readonly ICompanyRepository _companies;

        public FindCompaniesQueryHandler(ICompanyRepository companies)
        {
            _companies = companies;
        }

        public async Task Consume(ConsumeContext<FindCompaniesQuery> context)
        {
            var result = new FindCompaniesQueryResult { Results = new List<Company>() } ;
            try
            {
                if (context.Message.CompanyId.HasValue)
                {
                    var company = await _companies.Find(context.Message.CompanyId.Value);
                    if (company != null)
                        result.Results.Add(company);
                }
                else
                {
                    var companies = await _companies.FindAll();
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
