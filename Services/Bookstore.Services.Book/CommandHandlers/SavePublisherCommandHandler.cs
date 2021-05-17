using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class SavePublisherCommandHandler : IConsumer<SavePublisherCommand>
    {
        private readonly IPublisherRepository _publishers;

        public SavePublisherCommandHandler(IPublisherRepository publishers)
        {
            _publishers = publishers;
        }
        
        public async Task Consume(ConsumeContext<SavePublisherCommand> context)
        {
            var result = new SavePublisherCommandResult();
            try
            {
                result.Publisher = await _publishers.SavePublisher(context.Message.Publisher);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save publisher", ex);
            }
            await context.RespondAsync(result);
        }
    }
}