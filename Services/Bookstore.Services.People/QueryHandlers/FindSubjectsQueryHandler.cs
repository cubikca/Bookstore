using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindSubjectsQueryHandler : QueryHandlerBase<FindSubjectsQuery, FindSubjectsQueryResult, Subject>
    {
        private readonly ISubjectRepository _subjects;

        public FindSubjectsQueryHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ISubjectRepository subjects) : base(connection, mqOptions)
        {
            _subjects = subjects;
        }

        public override async Task<FindSubjectsQueryResult> Handle(FindSubjectsQuery request, CancellationToken cancellationToken)
        {
            var result = new FindSubjectsQueryResult {CorrelationId = request.Id, Results = new List<Subject>()};
            try
            {
                if (request.SubjectId.HasValue)
                {
                    var subject = await _subjects.FindSubjectById(request.SubjectId.Value);
                    if (subject != null)
                        result.Results.Add(subject);
                }
                else
                {
                    var subjects = await _subjects.FindAllSubjects();
                    result.Results = subjects;
                }
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
