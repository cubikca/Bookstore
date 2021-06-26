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
    public class FindOrganizationsQueryHandler : IConsumer<FindOrganizationsQuery>
    {
        private readonly IOrganizationRepository _organizations;
        private readonly IMessageDataRepository _messageData;

        public FindOrganizationsQueryHandler(IOrganizationRepository organizations, IMessageDataRepository messageData)
        {
            _organizations = organizations;
            _messageData = messageData;
        }

        public async Task Consume(ConsumeContext<FindOrganizationsQuery> context)
        {
            var result = new FindOrganizationsQueryResult();
            var organizations = Enumerable.Empty<Organization>().ToList();
            try
            {
                if (context.Message.CompanyId.HasValue)
                {
                    var company = await _organizations.Find(context.Message.CompanyId.Value);
                    if (company != null)
                        organizations.Add(company);
                }
                else
                    organizations = (await _organizations.FindAll()).ToList();
                var json = JsonConvert.SerializeObject(organizations);
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
