using System;
using System.Collections.Generic;
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

namespace Bookstore.Services.Book.QueryHandlers
{
    public class FindPublishersQueryHandler : IConsumer<FindPublishersQuery>
    {
        private readonly IPublisherRepository _publishers;
        private readonly IRequestClient<FindSubjectsQuery> _findSubjectsQuery;

        public FindPublishersQueryHandler(IPublisherRepository publishers, IPeopleBus peopleBus)
        {
            _publishers = publishers;
            _findSubjectsQuery = peopleBus.CreateRequestClient<FindSubjectsQuery>();
        }
        
        public async Task Consume(ConsumeContext<FindPublishersQuery> context)
        {
            var result = new FindPublishersQueryResult();
            try
            {
                if (context.Message.PublisherId.HasValue)
                {
                    var publisher = await _publishers.Find(context.Message.PublisherId.Value);
                    if (publisher != null)
                        result.Results = new List<Publisher> {publisher};
                    else
                        result.Results = Enumerable.Empty<Publisher>().ToList();
                }
                else
                    result.Results = (await _publishers.FindAll()).ToList();
                var tasks = result.Results.Select(async r =>
                {
                    var response = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                        new FindSubjectsQuery {SubjectId = r.ProfileId});
                    var profile = response.Message.Results.SingleOrDefault();
                    r.Profile = profile;
                });
                await Task.WhenAll(tasks);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to find publishers", ex);
            }
            await context.RespondAsync(result);
        }
    }
}