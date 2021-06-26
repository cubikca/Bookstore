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
using MassTransit.MessageData;
using Newtonsoft.Json;

namespace Bookstore.Services.People.QueryHandlers
{
    public class FindSubjectsQueryHandler : IConsumer<FindSubjectsQuery>
    {
        private readonly IMessageDataRepository _messageData;
        private readonly ISubjectRepository _subjects;

        public FindSubjectsQueryHandler(ISubjectRepository subjects, IMessageDataRepository messageData)
        {
            _subjects = subjects;
            _messageData = messageData;
        }

        public async Task Consume(ConsumeContext<FindSubjectsQuery> context)
        {
            var result = new FindSubjectsQueryResult();
            try
            {
                var subjects = Enumerable.Empty<Subject>().ToList();
                if (context.Message.SubjectId.HasValue)
                {
                    var subject = await _subjects.Find(context.Message.SubjectId.Value);
                    if (subject != null)
                        subjects.Add(subject);
                }
                else
                    subjects = (await _subjects.FindAll()).ToList();
                var json = JsonConvert.SerializeObject(subjects, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                result.Results = await _messageData.PutString(json);
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
