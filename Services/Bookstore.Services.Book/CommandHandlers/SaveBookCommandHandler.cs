using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SaveBookCommandHandler : IConsumer<SaveBookCommand>
    {
        private readonly IBookRepository _books;
        private readonly IPublisherRepository _publishers;
        private readonly IAuthorRepository _authors;
        private readonly IRequestClient<SaveSubjectCommand> _saveSubjectCommand;
        private readonly IRequestClient<RemoveSubjectCommand> _removeSubjectCommand;

        public SaveBookCommandHandler(IBookRepository books, IPublisherRepository publishers, IAuthorRepository authors, 
            IRequestClient<SaveSubjectCommand> saveSubjectCommand, IRequestClient<RemoveSubjectCommand> removeSubjectCommand)
        {
            _books = books;
            _publishers = publishers;
            _authors = authors;
            _saveSubjectCommand = saveSubjectCommand;
            _removeSubjectCommand = removeSubjectCommand;
        }
        
        public async Task Consume(ConsumeContext<SaveBookCommand> context)
        {
            var result = new SaveBookCommandResult();
            try
            {
                var authors = new List<Author>();
                Publisher publisher;
                if (context.Message.Book.Publisher != null)
                {
                    publisher = await _publishers.Save(context.Message.Book.Publisher);
                    if (context.Message.Book.Publisher.Profile != null)
                    {
                        var saveProfileResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                            new SaveSubjectCommand { Subject = context.Message.Book.Publisher.Profile });
                        publisher.Profile = saveProfileResponse.Message.Subject;
                    }
                }
                else
                {
                    publisher = await _publishers.FindPublisherForBook(context.Message.Book.Id);
                    if (publisher != null)
                    {
                        if (publisher.ProfileId != null)
                            await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                                new RemoveSubjectCommand { SubjectId = publisher.ProfileId.Value });
                        await _publishers.Remove(publisher.Id);
                    }
                }
                if (context.Message.Book.Authors != null)
                {
                    var bookAuthors = await _authors.FindByBook(context.Message.Book.Id);
                    foreach (var author in bookAuthors.ToList())
                    {
                        if (context.Message.Book.Authors.All(a => a.Id != author.Id))
                            await _authors.Remove(author.Id);
                    }
                    var saveProfileTasks = context.Message.Book.Authors.Select(async author =>
                    {
                        if (author.Profile != null)
                        {
                            var saveResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                                new SaveSubjectCommand { Subject = author.Profile });
                            author.Profile = saveResponse.Message.Subject;
                        }
                        lock (authors)
                            authors.Add(author);
                    });
                    await Task.WhenAll(saveProfileTasks);
                }
                else
                {
                    var bookAuthors = await _authors.FindByBook(context.Message.Book.Id);
                    foreach (var author in bookAuthors.ToList())
                    {
                        if (author.ProfileId != null)
                        {
                            await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                                new RemoveSubjectCommand { SubjectId = author.ProfileId.Value });
                        }
                        await _authors.Remove(author.Id);
                    }
                }
                result.Book = await _books.Save(context.Message.Book);
                result.Book.Authors = authors;
                result.Book.Publisher = publisher;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}