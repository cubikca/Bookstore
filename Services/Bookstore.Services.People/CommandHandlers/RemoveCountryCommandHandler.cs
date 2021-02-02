using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;

namespace Bookstore.Services.People.CommandHandlers
{
    public class RemoveCountryCommandHandler : IConsumer<RemoveCountryCommand>
    {
        private readonly ICountryRepository _countries;

        public RemoveCountryCommandHandler(ICountryRepository countries)
        {
            _countries = countries;
        }

        public async Task Consume(ConsumeContext<RemoveCountryCommand> context)
        {
            var result = new RemoveCountryCommandResult();
            try
            {
                result.Success = await _countries.RemoveCountry(context.Message.CountryId);
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
