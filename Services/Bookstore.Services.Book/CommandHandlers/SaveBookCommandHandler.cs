using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class SaveBookCommandHandler : IConsumer<SaveBookCommand>
    {
        private readonly IBookRepository _books;

        public SaveBookCommandHandler(IBookRepository books)
        {
            _books = books;
        }
        
        public async Task Consume(ConsumeContext<SaveBookCommand> context)
        {
            var result = new SaveBookCommandResult();
            try
            {
                result.Book = await _books.SaveBook(context.Message.Book);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save book", ex);
            }
            await context.RespondAsync(result);
        }
    }
}