using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RabbitWarren;

namespace Bookstore.Services.Workers.People
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQConsumer _consumer;

        public Worker(RabbitMQConnection rmqConn, ILogger<Worker> logger)
        {
            _logger = logger;
            var consumerChannel = rmqConn.OpenConsumerChannel("rabbitwarren", "people", exclusive: false);
            _consumer = consumerChannel.RegisterMediatRConsumer(Assembly.Load("Bookstore.Services.People"));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Start(autoDelete: false);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
