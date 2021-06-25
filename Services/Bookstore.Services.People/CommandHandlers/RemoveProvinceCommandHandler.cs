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
        private readonly IProvinceRepository _provinces;

        public RemoveProvinceCommandHandler(IProvinceRepository provinces)
        {
            _provinces = provinces;
        }

        public async Task Consume(ConsumeContext<RemoveProvinceCommand> context)
        {
            var result = new RemoveProvinceCommandResult();
            try
            {
                result.Success = await _provinces.Remove(context.Message.ProvinceId);
            }
            catch (Exception ex)
            {
                result.Error = ex.GetBaseException().Message;
            }
            await context.RespondAsync(result);
        }
    }
}
