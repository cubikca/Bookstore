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
    public class RemoveCountryCommandHandler : CommandHandlerBase<RemoveCountryCommand, RemoveCountryCommandResult>
    {
        private readonly ICountryRepository _countries;

        public RemoveCountryCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<RemoveCountryCommandResult> Handle(RemoveCountryCommand request, CancellationToken cancellationToken)
        {
            var result = new RemoveCountryCommandResult {CorrelationId = request.Id};
            try
            {
                result.Success = await _countries.RemoveCountry(request.CountryId);
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
