using System;
using System.Threading.Tasks;
using System.Transactions;
using Bookstore.Domains.Book;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class RemovePublisherCommandHandler : IConsumer<RemovePublisherCommand>
    {
        private readonly IPublisherRepository _publishers;
        private readonly IRequestClient<RemoveSubjectCommand> _removeSubjectCommand;

        public RemovePublisherCommandHandler(IPublisherRepository publishers, IPeopleBus peopleBus)
        {
            _publishers = publishers;
            _removeSubjectCommand = peopleBus.CreateRequestClient<RemoveSubjectCommand>();
        }
        
        public async Task Consume(ConsumeContext<RemovePublisherCommand> context)
        {
            var result = new RemovePublisherCommandResult();
            try
            {
                var publisher = await _publishers.Find(context.Message.PublisherId);
                var profileRemoved = true;
                if (publisher?.ProfileId != null)
                {
                    var removeSubjectResponse = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                        new RemoveSubjectCommand {SubjectId = publisher.ProfileId.Value});
                    profileRemoved = removeSubjectResponse.Message.Success;
                }
                var publisherRemoved = await _publishers.Remove(context.Message.PublisherId);
                result.Success = profileRemoved && publisherRemoved;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new BookException("Failed to remove publisher", ex);
            }
            await context.RespondAsync(result);
        }
    }
}