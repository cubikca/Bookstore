using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class SaveAuthorCommandHandler : IConsumer<SaveAuthorCommand>
    {
        private readonly IAuthorRepository _authors;

        public SaveAuthorCommandHandler(IAuthorRepository authors)
        {
            _authors = authors;
        }
        
        public async Task Consume(ConsumeContext<SaveAuthorCommand> context)
        {
            var result = new SaveAuthorCommandResult();
            try
            {
                result.Author = await _authors.Save(context.Message.Author);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save author", ex);
            }
            await context.RespondAsync(result);
        }
    }
}