using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class RemoveAuthorCommandHandler : IConsumer<RemoveAuthorCommand>
    {
        private readonly IAuthorRepository _authors;

        public RemoveAuthorCommandHandler(IAuthorRepository authors)
        {
            _authors = authors;
        }
        
        public async Task Consume(ConsumeContext<RemoveAuthorCommand> context)
        {
            var result = new RemoveAuthorCommandResult();
            try
            {
                result.Success = await _authors.Remove(context.Message.AuthorId);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to remove author", ex);
            }
            await context.RespondAsync(result);
        }
    }
}