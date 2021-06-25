using System;
using System.Collections.Generic;
using System.IO;
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
using MassTransit.MessageData;
using MassTransit.MultiBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindAuthorsQueryHandler : IConsumer<FindAuthorsQuery>
    {
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;
        private readonly IMessageDataRepository _messageData;

        public FindAuthorsQueryHandler(IAuthorRepository authors, IRequestClient<FindSubjectsQuery> findSubjectsQuery, IMessageDataRepository messageData)
        {
            _authors = authors;
            _findSubjectsQuery = findSubjectsQuery;
            _messageData = messageData;
        }
        
        public async Task Consume(ConsumeContext<FindAuthorsQuery> context)
        {
            var result = new FindAuthorsQueryResult();
            try
            {
                var authors = new List<Author>();
                if (context.Message.AuthorId.HasValue)
                {
                    var author = await _authors.Find(context.Message.AuthorId.Value);
                    if (author != null)
                        authors.Add(author);
                }
                else
                    authors.AddRange(await _authors.FindAll());
                var tasks = authors.Select(async a =>
                {
                    if (a.ProfileId != null)
                    {
                        var profileResponse =
                            await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(new FindSubjectsQuery
                                { SubjectId = a.ProfileId.Value });
                        var profile = profileResponse.Message.Results.SingleOrDefault();
                        a.Profile = profile;
                    }
                });
                await Task.WhenAll(tasks);
                // The authors list is possibly too large to fit into a 256K message, so we will serialize its BSON
                // data into a byte array and store it in an Azure blob. The API layer will have to retrieve the BSON
                // data and unwrap it into domain objects
                var json = JsonConvert.SerializeObject(authors);
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