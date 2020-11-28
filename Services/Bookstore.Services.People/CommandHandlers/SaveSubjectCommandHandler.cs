using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveSubjectCommandHandler : CommandHandlerBase<SaveSubjectCommand, SaveSubjectCommandResult>
    {
        private readonly IPersonRepository _people;
        private readonly ICompanyRepository _companies;

        public SaveSubjectCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, IPersonRepository people, ICompanyRepository companies) : base(connection, mqOptions)
        {
            _people = people;
            _companies = companies;
        }

        public override async Task<SaveSubjectCommandResult> Handle(SaveSubjectCommand request, CancellationToken cancellationToken)
        {
            Subject saved = null;
            var result = new SaveSubjectCommandResult {CorrelationId = request.Id};
            try
            {
                if (request.Subject is Person person)
                    saved = await _people.SavePerson(person);
                else if (request.Subject is Company company)
                    saved = await _companies.SaveCompany(company);
                result.Subject = saved;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
                result.Exception = ex;
            }
            return result;
        }
    }
}
