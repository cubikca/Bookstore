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
    public class RemoveSubjectCommandHandler : CommandHandlerBase<RemoveSubjectCommand, RemoveSubjectCommandResult>
    {
        private readonly IPersonRepository _people;
        private readonly ICompanyRepository _companies;
        private readonly ISubjectRepository _subjects;

        public RemoveSubjectCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ISubjectRepository subjects, IPersonRepository people, ICompanyRepository companies) : base(connection, mqOptions)
        {
            _people = people;
            _subjects = subjects;
            _companies = companies;
        }

        public override async Task<RemoveSubjectCommandResult> Handle(RemoveSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new RemoveSubjectCommandResult {CorrelationId = request.Id};
            try
            {
                var subject = await _subjects.FindSubjectById(request.SubjectId);
                if (subject != null)
                {
                    if (subject is Company company)
                        result.Success = await _companies.RemoveCompany(company.Id);
                    else if (subject is Person person)
                        result.Success = await _people.RemovePerson(person.Id);
                }
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
