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
    public class SaveProvinceCommandHandler : IConsumer<SaveProvinceCommand>
    {
        private readonly IProvinceRepository _provinces;

        public SaveProvinceCommandHandler(IProvinceRepository provinces)
        {
            _provinces = provinces;
        }

        public async Task Consume(ConsumeContext<SaveProvinceCommand> context)
        {
            var result = new SaveProvinceCommandResult();
            try
            {
                var province = await _provinces.Save(context.Message.Province);
                result.Province = province;
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
