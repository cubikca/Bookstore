using System;
using System.Threading.Tasks;
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

        public RemovePublisherCommandHandler(IPublisherRepository publishers, IRequestClient<RemoveSubjectCommand> removeSubjectCommand)
        {
            _publishers = publishers;
            _removeSubjectCommand = removeSubjectCommand;
        }
        
        public async Task Consume(ConsumeContext<RemovePublisherCommand> context)
        {
            var result = new RemovePublisherCommandResult();
            try
            {
                var publisher = await _publishers.Find(context.Message.PublisherId);
                var profileRemoved = publisher.Profile == null;
                if (publisher.Profile != null)
                {
                    var removeSubjectResponse = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                        new RemoveSubjectCommand { SubjectId = publisher.Profile.Id });
                    profileRemoved = removeSubjectResponse.Message.Success;
                }
                result.Success = await _publishers.Remove(context.Message.PublisherId) && profileRemoved;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}