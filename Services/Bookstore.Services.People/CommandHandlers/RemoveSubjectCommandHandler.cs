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
    public class RemoveSubjectCommandHandler : IConsumer<RemoveSubjectCommand>
    {
        private readonly ISubjectRepository _subjects;

        public RemoveSubjectCommandHandler(ISubjectRepository subjects)
        {
            _subjects = subjects;
        }

        public async Task Consume(ConsumeContext<RemoveSubjectCommand> context)
        {
            var result = new RemoveSubjectCommandResult();
            try
            {
                result.Success = await _subjects.Remove(context.Message.SubjectId);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}
