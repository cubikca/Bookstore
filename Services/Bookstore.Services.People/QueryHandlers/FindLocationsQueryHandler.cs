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
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindLocationsQueryHandler : IConsumer<FindLocationsQuery>
    {
        private readonly ILogger<FindLocationsQueryHandler> _logger;
        private readonly ILocationRepository _locations;

        public FindLocationsQueryHandler(ILocationRepository locations, ILogger<FindLocationsQueryHandler> logger)
        {
            _locations = locations;
            _logger = logger;
        }
        
        public async Task Consume(ConsumeContext<FindLocationsQuery> context)
        {
            var result = new FindLocationsQueryResult();
            try
            {
                result.Results = context.Message.LocationId.HasValue 
                    ? new List<Location> {await _locations.Find(context.Message.LocationId.Value)} 
                    : (await _locations.FindAll()).ToList();
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