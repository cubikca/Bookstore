using System.Threading;
using System.Threading.Tasks;
using Bookstore.Entities.Book;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.Book.Worker
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
            _logger.LogInformation("BookService starting...");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogInformation("BookService stopping...");
        }
    }
}
