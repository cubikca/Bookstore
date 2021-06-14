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
    public class SaveAuthorCommandHandler : IConsumer<SaveAuthorCommand>
    {
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<SaveSubjectCommand> _saveSubjectCommand;

        public SaveAuthorCommandHandler(IAuthorRepository authors, IPeopleBus peopleBus)
        {
            _authors = authors;
            _saveSubjectCommand = peopleBus.CreateRequestClient<SaveSubjectCommand>();
        }
        
        public async Task Consume(ConsumeContext<SaveAuthorCommand> context)
        {
            var result = new SaveAuthorCommandResult();
            try
            {
                result.Author = await _authors.Save(context.Message.Author);
                var saveSubjectResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                    new SaveSubjectCommand {Subject = context.Message.Author.Profile});
                result.Author.Profile = saveSubjectResponse.Message.Subject;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save author", ex);
            }
            await context.RespondAsync(result);
        }
    }
}