using System;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveLocationCommandHandler : IConsumer<SaveLocationCommand>
    {
        private readonly ILogger<SaveLocationCommandHandler> _logger;
        private readonly ILocationRepository _locations;

        public SaveLocationCommandHandler(ILocationRepository locations, ILogger<SaveLocationCommandHandler> logger)
        {
            _logger = logger;
            _locations = locations;
        }

        public async Task Consume(ConsumeContext<SaveLocationCommand> context)
        {
            var result = new SaveLocationCommandResult();
            try
            {
                result.Location = await _locations.Save(context.Message.Location);
                result.Success = true;
            }
            catch (Exception ex)
            {
                var msg = "Failed to save Entity of type Location";
                _logger.LogError(ex, msg);
                result.Error = msg;
            }
            await context.RespondAsync(result);
        }
    }
}