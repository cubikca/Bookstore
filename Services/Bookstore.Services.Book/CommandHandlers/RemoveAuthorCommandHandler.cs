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
    public class RemoveAuthorCommandHandler : IConsumer<RemoveAuthorCommand>
    {
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<RemoveSubjectCommand> _removeSubjectCommand;

        public RemoveAuthorCommandHandler(IAuthorRepository authors, IPeopleBus peopleBus)
        {
            _authors = authors;
            _removeSubjectCommand = peopleBus.CreateRequestClient<RemoveSubjectCommand>();
        }
        
        public async Task Consume(ConsumeContext<RemoveAuthorCommand> context)
        {
            var result = new RemoveAuthorCommandResult();
            try
            {
                var author = await _authors.Find(context.Message.AuthorId);
                var profileRemoved = true;
                if (author.ProfileId.HasValue)
                {
                    var removeSubjectResponse = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                     new RemoveSubjectCommand {SubjectId = author.ProfileId.Value});
                    profileRemoved = removeSubjectResponse.Message.Success;
                }
                var authorRemoved = await _authors.Remove(context.Message.AuthorId);
                result.Success = profileRemoved && authorRemoved;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new BookException("Failed to remove author", ex);
            }
            await context.RespondAsync(result);
        }
    }
}