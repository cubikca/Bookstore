using System;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveAddressCommandHandler : IConsumer<SaveAddressCommand>
    {
        private readonly IAddressRepository _addresses;
        private readonly ILogger<SaveAddressCommandHandler> _logger;

        public SaveAddressCommandHandler(IAddressRepository addresses, ILogger<SaveAddressCommandHandler> logger)
        {
            _logger = logger;
            _addresses = addresses;
        }
        
        public async Task Consume(ConsumeContext<SaveAddressCommand> context)
        {
            var result = new SaveAddressCommandResult();
            try
            {
                result.Address = await _addresses.Save(context.Message.Address);
                result.Success = true;
            }
            catch (Exception ex)
            {
                var msg = "Failed to save Entity of type Address";
                _logger.LogError(ex, msg);
                result.Error = msg;
            }
            await context.RespondAsync(result);
        }
    }
}