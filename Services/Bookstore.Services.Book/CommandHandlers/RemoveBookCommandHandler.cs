using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class RemoveBookCommandHandler : IConsumer<RemoveBookCommand>
    {
        private readonly IBookRepository _books;
        
        public RemoveBookCommandHandler(IBookRepository books)
        {
            _books = books;
        }
        
        public async Task Consume(ConsumeContext<RemoveBookCommand> context)
        {
            var result = new RemoveBookCommandResult();
            try
            {
                result.Success = await _books.Remove(context.Message.BookId);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to remove book", ex);
            }
            await context.RespondAsync(result);
        }
    }
}