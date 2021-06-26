using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.People;
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
    public class FindLocationsQueryHandler : IConsumer<FindLocationsQuery>
    {
        private readonly ILogger<FindLocationsQueryHandler> _logger;
        private readonly ILocationRepository _locations;
        private readonly IMessageDataRepository _messageData;
        
        public FindLocationsQueryHandler(ILocationRepository locations, ILogger<FindLocationsQueryHandler> logger, IMessageDataRepository messageData)
        {
            _locations = locations;
            _logger = logger;
            _messageData = messageData;
        }
        
        public async Task Consume(ConsumeContext<FindLocationsQuery> context)
        {
            var result = new FindLocationsQueryResult();
            try
            {
                var locations = Enumerable.Empty<Location>().ToList();
                if (context.Message.LocationId.HasValue)
                {
                    var location = await _locations.Find(context.Message.LocationId.Value);
                    if (location != null)
                        locations.Add(location);
                }
                else
                    locations.AddRange(await _locations.FindAll());
                var json = JsonConvert.SerializeObject(locations);
                result.Results = await _messageData.PutString(json);
            }
            catch (Exception ex)
            {
                var message =
                    $"Failed to retrieve Entit{(context.Message.LocationId.HasValue ? "y" : "ies")} of type Location";
                _logger.LogError(ex, message);
                result.Error = message;
            }
            await context.RespondAsync(result);
        }
    }
}