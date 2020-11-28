using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using RabbitWarren;
using RabbitWarren.ClientHandlers;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveCountryCommandHandler : CommandHandlerBase<SaveCountryCommand, SaveCountryCommandResult>
    {
        private readonly ICountryRepository _countries;

        public SaveCountryCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<SaveCountryCommandResult> Handle(SaveCountryCommand request, CancellationToken cancellationToken)
        {
            var result = new SaveCountryCommandResult {CorrelationId = request.Id};
            try
            {
                var country = await _countries.SaveCountry(request.Country);
                result.Country = country;
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
