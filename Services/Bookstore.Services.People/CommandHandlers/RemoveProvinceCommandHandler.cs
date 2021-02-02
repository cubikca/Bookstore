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
    public class RemoveProvinceCommandHandler : IConsumer<RemoveProvinceCommand>
    {
        private readonly ICountryRepository _countries;

        public RemoveProvinceCommandHandler(ICountryRepository countries)
        {
            _countries = countries;
        }

        public async Task Consume(ConsumeContext<RemoveProvinceCommand> context)
        {
            var result = new RemoveProvinceCommandResult();
            try
            {
                result.Success = await _countries.RemoveProvince(context.Message.ProvinceId);
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
