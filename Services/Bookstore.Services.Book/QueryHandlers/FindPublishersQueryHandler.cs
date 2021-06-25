using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Entities.Book;
using MassTransit;
using MassTransit.MessageData;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindPublishersQueryHandler : IConsumer<FindPublishersQuery>
    {
        private readonly IPublisherRepository _publishers;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;
        private readonly IMessageDataRepository _messageData;

        public FindPublishersQueryHandler(IPublisherRepository publishers, IRequestClient<FindSubjectsQuery> findSubjectsQuery, IMessageDataRepository messageData)
        {
            _publishers = publishers;
            _findSubjectsQuery = findSubjectsQuery;
            _messageData = messageData;
        }
        
        public async Task Consume(ConsumeContext<FindPublishersQuery> context)
        {
            var result = new FindPublishersQueryResult();
            try
            {
                var results = new List<Publisher>();
                if (context.Message.PublisherId.HasValue)
                {
                    var publisher = await _publishers.Find(context.Message.PublisherId.Value);
                    if (publisher != null) results.Add(publisher);
                }
                else
                    results.AddRange(await _publishers.FindAll());
                var tasks = results.Select(async r =>
                {
                    if (r.ProfileId != null)
                    {
                        var findSubjectResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                            new FindSubjectsQuery { SubjectId = r.ProfileId.Value });
                        r.Profile = findSubjectResponse.Message.Results.SingleOrDefault();
                    }
                });
                await Task.WhenAll(tasks);
                var json = JsonConvert.SerializeObject(results);
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