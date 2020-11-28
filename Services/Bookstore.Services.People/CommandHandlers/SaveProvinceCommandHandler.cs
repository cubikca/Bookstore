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
    public class SaveProvinceCommandHandler : CommandHandlerBase<SaveProvinceCommand, SaveProvinceCommandResult>
    {
        private readonly ICountryRepository _countries;

        public SaveProvinceCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<SaveProvinceCommandResult> Handle(SaveProvinceCommand request, CancellationToken cancellationToken)
        {
            var result = new SaveProvinceCommandResult {CorrelationId = request.Id};
            try
            {
                var province = await _countries.SaveProvince(request.Province);
                result.Province = province;
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
