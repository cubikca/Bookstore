using System;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.CommandHandlers
{
    public class RemoveLocationCommandHandler : IConsumer<RemoveLocationCommand>
    {
        private readonly ILogger<RemoveLocationCommandHandler> _logger;
        private readonly ILocationRepository _locations;

        public RemoveLocationCommandHandler(ILocationRepository locations, ILogger<RemoveLocationCommandHandler> logger)
        {
            _locations = locations;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RemoveLocationCommand> context)
        {
            var result = new RemoveLocationCommandResult();
            try
            {
                result.Success = await _locations.Remove(context.Message.LocationId);
            }
            catch (Exception ex)
            {
                var msg = "Failed to remove Entity of type Location";
                _logger.LogError(ex, msg);
                result.Error = msg;
            }
            await context.RespondAsync(result);
        }
    }
}