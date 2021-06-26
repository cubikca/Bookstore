using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using MassTransit.MessageData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindAddressesQueryHandler : IConsumer<FindAddressesQuery>
    {
        private readonly ILogger<FindAddressesQueryHandler> _logger;
        private readonly IAddressRepository _addresses;
        private readonly IMessageDataRepository _messageData;

        public FindAddressesQueryHandler(IAddressRepository addresses, ILogger<FindAddressesQueryHandler> logger, IMessageDataRepository messageData)
        {
            _addresses = addresses;
            _logger = logger;
            _messageData = messageData;
        }
        
        public async Task Consume(ConsumeContext<FindAddressesQuery> context)
        {
            var result = new FindAddressesQueryResult();
            try
            {
                List<Address> addresses = Enumerable.Empty<Address>().ToList();
                if (context.Message.AddressId.HasValue)
                {
                    var address = await _addresses.Find(context.Message.AddressId.Value);
                    if (address != null)
                        addresses.Add(address);
                }
                else
                    addresses.AddRange(await _addresses.FindAll());
                var json = JsonConvert.SerializeObject(addresses);
                result.Results = await _messageData.PutString(json);
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