using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindBooksQueryHandler : IConsumer<FindBooksQuery>
    {
        private readonly IBookRepository _books;

        public FindBooksQueryHandler(IBookRepository books)
        {
            _books = books;
        }
        
        public async Task Consume(ConsumeContext<FindBooksQuery> context)
        {
            var result = new FindBooksQueryResult();
            try
            {
                if (context.Message.BookId.HasValue)
                    result.Results = new List<Domains.Book.Models.Book>
                        {await _books.Find(context.Message.BookId.Value)};
                else
                    result.Results = (await _books.FindAll()).ToList();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to find books", ex);
            }
            await context.RespondAsync(result);
        }
    }
}