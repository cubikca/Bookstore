using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Entities.Book;
using MassTransit;
using MassTransit.MessageData;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindBooksQueryHandler : IConsumer<FindBooksQuery>
    {
        private readonly IBookRepository _books;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;
        private readonly IMessageDataRepository _messageData;

        public FindBooksQueryHandler(IBookRepository books, IRequestClient<FindSubjectsQuery> findSubjectsQuery, IMessageDataRepository messageData)
        {
            _books = books;
            _findSubjectsQuery = findSubjectsQuery;
            _messageData = messageData;
        }
        
        public async Task Consume(ConsumeContext<FindBooksQuery> context)
        {
            var result = new FindBooksQueryResult();
            var books = new List<Domains.Book.Models.Book>();
            try
            {
                if (context.Message.BookId.HasValue)
                {
                    var book = await _books.Find(context.Message.BookId.Value);
                    if (book != null) books.Add(book);
                }
                else
                    books.AddRange(await _books.FindAll());
                foreach (var book in books)
                {
                    if (book.Publisher?.ProfileId != null)
                    {
                        var profileResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                            new FindSubjectsQuery { SubjectId = book.Publisher.ProfileId.Value });
                        var publisherJson = await profileResponse.Message.Results.Value;
                        var profiles = JsonConvert.DeserializeObject<List<Subject>>(publisherJson, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects })
                            ?? Enumerable.Empty<Subject>().ToList();
                        book.Publisher.Profile = profiles.SingleOrDefault();
                    }
                    var fillAuthorProfilesTasks = book.Authors.Select(async author =>
                    {
                        if (author.ProfileId != null)
                        {
                            var profileResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                                new FindSubjectsQuery { SubjectId = author.ProfileId.Value });
                            var authorJson = await profileResponse.Message.Results.Value;
                            var profiles = JsonConvert.DeserializeObject<List<Subject>>(authorJson, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects })
                                ?? Enumerable.Empty<Subject>().ToList();
                            author.Profile = profiles.SingleOrDefault();
                        }
                    });
                    await Task.WhenAll(fillAuthorProfilesTasks);
                }
                var json = JsonConvert.SerializeObject(books);
                result.Results = await _messageData.PutString(json);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}