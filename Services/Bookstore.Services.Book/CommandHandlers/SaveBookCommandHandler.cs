using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Bookstore.Domains.Book.CommandResults;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Entities.Book;
using MassTransit;

namespace Bookstore.Services.Book.CommandHandlers
{
    public class SaveBookCommandHandler : IConsumer<SaveBookCommand>
    {
        private readonly IBookRepository _books;
        private readonly IRequestClient<SaveSubjectCommand> _saveSubjectCommand;

        public SaveBookCommandHandler(IBookRepository books, IBus bus, IPeopleBus peopleBus)
        {
            _books = books;
            _saveSubjectCommand = peopleBus.CreateRequestClient<SaveSubjectCommand>();
        }
        
        public async Task Consume(ConsumeContext<SaveBookCommand> context)
        {
            var result = new SaveBookCommandResult();
            try
            {
                result.Book = await _books.Save(context.Message.Book);
                var savePublisherResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                    new SaveSubjectCommand {Subject = context.Message.Book.Publisher.Profile});
                var authorProfileTasks = result.Book.Authors.Select(async a =>
                {
                    var model = context.Message.Book.Authors.SingleOrDefault(m => m.Id == a.Id);
                    if (model != null)
                    {
                        var response = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                            new SaveSubjectCommand {Subject = model.Profile});
                        a.Profile = response.Message.Subject;
                    }
                });
                await Task.WhenAll(authorProfileTasks);
                result.Book.Publisher.Profile = savePublisherResponse.Message.Subject;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = new EntityException("Failed to save book", ex);
            }
            await context.RespondAsync(result);
        }
    }
}