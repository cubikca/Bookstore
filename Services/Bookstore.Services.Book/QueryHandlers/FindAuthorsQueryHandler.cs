using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Queries;
using Bookstore.Entities.Book;
using MassTransit;
using MassTransit.MultiBus;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindAuthorsQueryHandler : IConsumer<FindAuthorsQuery>
    {
        private readonly IAuthorRepository _authors;
        private readonly IPeopleBus _peopleBus;

        public FindAuthorsQueryHandler(IAuthorRepository authors, IPeopleBus peopleBus)
        {
            _authors = authors;
            _peopleBus = peopleBus;
        }
        
        public async Task Consume(ConsumeContext<FindAuthorsQuery> context)
        {
            var result = new FindAuthorsQueryResult();
            try
            {
                if (context.Message.AuthorId.HasValue)
                    result.Results = new List<Author> {await _authors.Find(context.Message.AuthorId.Value)};
                else
                    result.Results = (await _authors.FindAll()).ToList();
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