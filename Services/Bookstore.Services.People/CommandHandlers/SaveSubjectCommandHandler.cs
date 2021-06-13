using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using MassTransit;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveSubjectCommandHandler : IConsumer<SaveSubjectCommand>
    {
        private readonly ISubjectRepository _subjects;

        public SaveSubjectCommandHandler(ISubjectRepository subjects)
        {
            _subjects = subjects;
        }

        public async Task Consume(ConsumeContext<SaveSubjectCommand> context)
        {
            var result = new SaveSubjectCommandResult();
            try
            {
                result.Subject = await _subjects.Save(context.Message.Subject);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = ex;
            }
            await context.RespondAsync(result);
        }
    }
}
