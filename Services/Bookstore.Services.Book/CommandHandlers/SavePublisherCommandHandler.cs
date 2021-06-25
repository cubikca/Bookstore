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
    public class SavePublisherCommandHandler : IConsumer<SavePublisherCommand>
    {
        private readonly IPublisherRepository _publishers;
        private readonly IRequestClient<SaveSubjectCommand> _saveSubjectCommand;

        public SavePublisherCommandHandler(IPublisherRepository publishers, IRequestClient<SaveSubjectCommand> saveSubjectCommand)
        {
            _publishers = publishers;
            _saveSubjectCommand = saveSubjectCommand;
        }
        
        public async Task Consume(ConsumeContext<SavePublisherCommand> context)
        {
            var result = new SavePublisherCommandResult();
            try
            {
                result.Publisher = await _publishers.Save(context.Message.Publisher);
                var profileSaved = context.Message.Publisher.Profile == null;
                if (context.Message.Publisher.Profile != null)
                {
                    var profileResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                        new SaveSubjectCommand { Subject = context.Message.Publisher.Profile });
                    result.Publisher.Profile = profileResponse.Message.Subject;
                    profileSaved = profileResponse.Message.Success;
                }
                result.Success = profileSaved;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}