using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindAuthorsQueryHandler : IConsumer<FindAuthorsQuery>
    {
        private readonly IAuthorRepository _authors;

        public FindAuthorsQueryHandler(IAuthorRepository authors)
        {
            _authors = authors;
        }
        
        public async Task Consume(ConsumeContext<FindAuthorsQuery> context)
        {
            var result = new FindAuthorsQueryResult();
            try
            {
                if (context.Message.AuthorId.HasValue)
                    result.Results = new List<Author> {await _authors.FindAuthorById(context.Message.AuthorId.Value)};
                else
                    result.Results = await _authors.FindAllAuthors();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to find authors", ex);
            }
            await context.RespondAsync(result);
        }
    }
}