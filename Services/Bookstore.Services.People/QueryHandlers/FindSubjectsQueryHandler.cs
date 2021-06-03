using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using MassTransit;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindSubjectsQueryHandler : IConsumer<FindSubjectsQuery>
    {
        private readonly ISubjectRepository _subjects;

        public FindSubjectsQueryHandler(ISubjectRepository subjects)
        {
            _subjects = subjects;
        }

        public async Task Consume(ConsumeContext<FindSubjectsQuery> context)
        {
            var result = new FindSubjectsQueryResult {Results = new List<Subject>()};
            try
            {
                if (context.Message.SubjectId.HasValue)
                {
                    var subject = await _subjects.Find(context.Message.SubjectId.Value);
                    if (subject != null)
                        result.Results.Add(subject);
                }
                else
                {
                    var subjects = await _subjects.FindAll();
                    result.Results = subjects.ToList();
                }
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
