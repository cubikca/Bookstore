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
    public class RemoveAuthorCommandHandler : IConsumer<RemoveAuthorCommand>
    {
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<RemoveSubjectCommand> _removeSubjectCommand;

        public RemoveAuthorCommandHandler(IAuthorRepository authors, IRequestClient<RemoveSubjectCommand> removeSubjectCommand)
        {
            _authors = authors;
            _removeSubjectCommand = removeSubjectCommand;
        }
        
        public async Task Consume(ConsumeContext<RemoveAuthorCommand> context)
        {
            var result = new RemoveAuthorCommandResult();
            try
            {
                var author = await _authors.Find(context.Message.AuthorId);
                var profileRemoved = author.ProfileId == null;
                if (author.ProfileId != null)
                {
                    var removeSubjectResult = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                        new RemoveSubjectCommand { SubjectId = author.ProfileId.Value });
                    profileRemoved = removeSubjectResult.Message.Success;
                }
                result.Success = await _authors.Remove(context.Message.AuthorId) && profileRemoved;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}