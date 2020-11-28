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
    public class RemoveProvinceCommandHandler : CommandHandlerBase<RemoveProvinceCommand, RemoveProvinceCommandResult>
    {
        private readonly ICountryRepository _countries;

        public RemoveProvinceCommandHandler(RabbitMQConnection connection, RabbitMQOptions mqOptions, ICountryRepository countries) : base(connection, mqOptions)
        {
            _countries = countries;
        }

        public override async Task<RemoveProvinceCommandResult> Handle(RemoveProvinceCommand request, CancellationToken cancellationToken)
        {
            var result = new RemoveProvinceCommandResult {CorrelationId = request.Id};
            try
            {
                result.Success = await _countries.RemoveProvince(request.ProvinceId);
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
