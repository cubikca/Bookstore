using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class RemovePublisherCommandHandler : IConsumer<RemovePublisherCommand>
    {
        private readonly IPublisherRepository _publishers;

        public RemovePublisherCommandHandler(IPublisherRepository publishers)
        {
            _publishers = publishers;
        }
        
        public async Task Consume(ConsumeContext<RemovePublisherCommand> context)
        {
            var result = new RemovePublisherCommandResult();
            try
            {
                result.Success = await _publishers.Remove(context.Message.PublisherId);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to remove publisher", ex);
            }
            await context.RespondAsync(result);
        }
    }
}