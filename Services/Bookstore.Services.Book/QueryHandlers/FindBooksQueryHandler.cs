using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindBooksQueryHandler : IConsumer<FindBooksQuery>
    {
        private readonly IBookRepository _books;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;

        public FindBooksQueryHandler(IBookRepository books, IPeopleBus peopleBus)
        {
            _books = books;
            _findSubjectsQuery = peopleBus.CreateRequestClient<FindSubjectsQuery>();
        }
        
        public async Task Consume(ConsumeContext<FindBooksQuery> context)
        {
            var result = new FindBooksQueryResult();
            try
            {
                if (context.Message.BookId.HasValue)
                {
                    var book = await _books.Find(context.Message.BookId.Value);
                    result.Results = book != null ? new List<Domains.Book.Models.Book> {book} : Enumerable.Empty<Domains.Book.Models.Book>().ToList();
                }
                else
                    result.Results = (await _books.FindAll()).ToList();
                var publisherTasks = result.Results.Select(async r =>
                {
                    if (r.Publisher?.ProfileId != null)
                    {
                        var profileResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                            new FindSubjectsQuery {SubjectId = r.Publisher.ProfileId});
                        r.Publisher.Profile = profileResponse.Message.Results.SingleOrDefault();
                    }
                });
                await Task.WhenAll(publisherTasks);
                var authorTasks = result.Results.Select(async r =>
                {
                    foreach (var author in r.Authors)
                    {
                        if (author.ProfileId != null)
                        {
                            var profileResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                                new FindSubjectsQuery {SubjectId = author.ProfileId});
                            author.Profile = profileResponse.Message.Results.SingleOrDefault();
                        }
                    }
                });
                await Task.WhenAll(authorTasks);
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