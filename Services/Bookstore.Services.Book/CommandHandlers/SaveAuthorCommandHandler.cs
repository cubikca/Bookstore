using System;
using System.Threading.Tasks;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Models;
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

        public SaveAuthorCommandHandler(IAuthorRepository authors, IRequestClient<SaveSubjectCommand> saveSubjectCommand)
        {
            _authors = authors;
            _saveSubjectCommand = saveSubjectCommand;
        }
        
        public async Task Consume(ConsumeContext<SaveAuthorCommand> context)
        {
            var result = new SaveAuthorCommandResult();
            try
            {
                result.Author = await _authors.Save(context.Message.Author);
                var profileSaved = context.Message.Author.Profile == null;
                if (context.Message.Author.Profile != null)
                {
                    var profileResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                        new SaveSubjectCommand { Subject = context.Message.Author.Profile });
                    profileSaved = profileResponse.Message.Success;
                    result.Author.Profile = profileResponse.Message.Subject;
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