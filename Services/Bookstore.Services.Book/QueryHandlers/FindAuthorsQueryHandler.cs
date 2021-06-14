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
using Bookstore.Domains.People.QueryResults;
using Bookstore.Entities.Book;
using MassTransit;
using MassTransit.MultiBus;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindAuthorsQueryHandler : IConsumer<FindAuthorsQuery>
    {
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;

        public FindAuthorsQueryHandler(IAuthorRepository authors, IPeopleBus peopleBus)
        {
            _authors = authors;
            _findSubjectsQuery = peopleBus.CreateRequestClient<FindSubjectsQuery>();
        }
        
        public async Task Consume(ConsumeContext<FindAuthorsQuery> context)
        {
            var result = new FindAuthorsQueryResult();
            try
            {
                if (context.Message.AuthorId.HasValue)
                {
                    var author = await _authors.Find(context.Message.AuthorId.Value);
                    if (author != null)
                        result.Results = new List<Author> {await _authors.Find(context.Message.AuthorId.Value)};
                    else
                        result.Results = Enumerable.Empty<Author>().ToList();
                }
                else
                    result.Results = (await _authors.FindAll()).ToList();
                var tasks = result.Results.Select(async r =>
                {
                    if (r != null && r.ProfileId != null)
                    {
                        var response = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                            new FindSubjectsQuery {SubjectId = r.ProfileId});
                        r.Profile = response.Message.Results.SingleOrDefault();
                    }
                });
                await Task.WhenAll(tasks);
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