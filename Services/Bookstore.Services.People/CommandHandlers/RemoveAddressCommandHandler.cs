using System;
using System.Threading.Tasks;
using Bookstore.Domains.People;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.People.CommandHandlers
{
    public class RemoveAddressCommandHandler : IConsumer<RemoveAddressCommand>
    {
        private readonly IAddressRepository _addresses;
        private readonly ILogger<RemoveAddressCommandHandler> _logger;
        
        public RemoveAddressCommandHandler(IAddressRepository addresses, ILogger<RemoveAddressCommandHandler> logger)
        {
            _addresses = addresses;
            _logger = logger;
        }
        
        public async Task Consume(ConsumeContext<RemoveAddressCommand> context)
        {
            var result = new RemoveAddressCommandResult();
            try
            {
                result.Success = await _addresses.Remove(context.Message.AddressId);
            }
            catch (Exception ex)
            {
                var msg = "Failed to save Entity of type Address";
                _logger.LogError(ex, msg);
                result.Error = msg;
                result.Exception = ex;
            }
            await context.RespondAsync(result);
        }
    }
}