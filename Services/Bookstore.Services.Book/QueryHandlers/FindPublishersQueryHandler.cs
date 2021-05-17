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
    public class FindPublishersQueryHandler : IConsumer<FindPublishersQuery>
    {
        private readonly IPublisherRepository _publishers;

        public FindPublishersQueryHandler(IPublisherRepository publishers)
        {
            _publishers = publishers;
        }
        
        public async Task Consume(ConsumeContext<FindPublishersQuery> context)
        {
            var result = new FindPublishersQueryResult();
            try
            {
                if (context.Message.PublisherId.HasValue)
                    result.Results = new List<Publisher>
                        {await _publishers.FindPublisherById(context.Message.PublisherId.Value)};
                else
                    result.Results = await _publishers.FindAllPublishers();
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