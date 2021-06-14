using System;
using System.Threading.Tasks;
using System.Transactions;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class SavePublisherCommandHandler : IConsumer<SavePublisherCommand>
    {
        private readonly IPublisherRepository _publishers;
        private readonly IRequestClient<SaveSubjectCommand> _saveSubjectCommand;

        public SavePublisherCommandHandler(IPublisherRepository publishers, IPeopleBus peopleBus)
        {
            _publishers = publishers;
            _saveSubjectCommand = peopleBus.CreateRequestClient<SaveSubjectCommand>();
        }
        
        public async Task Consume(ConsumeContext<SavePublisherCommand> context)
        {
            var result = new SavePublisherCommandResult();
            try
            {
                result.Publisher = await _publishers.Save(context.Message.Publisher);
                var saveSubjectResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                        new SaveSubjectCommand { Subject = context.Message.Publisher.Profile});
                result.Publisher.Profile = saveSubjectResponse.Message.Subject;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save publisher", ex);
            }
            await context.RespondAsync(result);
        }
    }
}