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
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveSubjectCommandHandler : IConsumer<SaveSubjectCommand>
    {
        private readonly IPersonRepository _people;
        private readonly ICompanyRepository _companies;

        public SaveSubjectCommandHandler(IPersonRepository people, ICompanyRepository companies)
        {
            _people = people;
            _companies = companies;
        }

        public async Task Consume(ConsumeContext<SaveSubjectCommand> context)
        {
            Subject saved = null;
            var result = new SaveSubjectCommandResult();
            try
            {
                if (context.Message.Subject is Person person)
                    saved = await _people.SavePerson(person);
                else if (context.Message.Subject is Company company)
                    saved = await _companies.SaveCompany(company);
                result.Subject = saved;
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
