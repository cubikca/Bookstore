using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Repositories;
using MassTransit;

namespace Bookstore.Services.People.CommandHandlers
{
    public class SaveCountryCommandHandler : IConsumer<SaveCountryCommand>
    {
        private readonly ICountryRepository _countries;

        public SaveCountryCommandHandler(ICountryRepository countries)
        {
            _countries = countries;
        }

        public async Task Consume(ConsumeContext<SaveCountryCommand> context)
        {
            var result = new SaveCountryCommandResult();
            try
            {
                var country = await _countries.SaveCountry(context.Message.Country);
                result.Country = country;
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
