using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Entities.People;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Services.Workers.People
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PeopleService starting...");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogInformation("PeopleService stopping...");
        }
    }
}
