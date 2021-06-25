using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindAddressesQueryHandler : IConsumer<FindAddressesQuery>
    {
        private readonly ILogger<FindAddressesQueryHandler> _logger;
        private readonly IAddressRepository _addresses;

        public FindAddressesQueryHandler(IAddressRepository addresses, ILogger<FindAddressesQueryHandler> logger)
        {
            _addresses = addresses;
            _logger = logger;
        }
        
        public async Task Consume(ConsumeContext<FindAddressesQuery> context)
        {
            var result = new FindAddressesQueryResult();
            try
            {
                result.Results = context.Message.AddressId.HasValue
                    ? new List<Address> {await _addresses.Find(context.Message.AddressId.Value)}
                    : (await _addresses.FindAll()).ToList();
            }
            catch (Exception ex)
            {
                var msg = $"Failed to retrieve Entit{(context.Message.AddressId.HasValue ? "y" : "ies")}";
                _logger.LogError(ex, msg);
            }
            await context.RespondAsync(result);
        }
    }
}