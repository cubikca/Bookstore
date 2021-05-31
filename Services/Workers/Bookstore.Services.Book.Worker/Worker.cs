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
        private readonly IBusControl _busControl;
        private readonly IDbContextFactory<BookContext> _dbFactory;

        public Worker(IBusControl busControl, ILogger<Worker> logger, IDbContextFactory<BookContext> dbFactory)
        {
            _logger = logger;
            _busControl = busControl;
            _dbFactory = dbFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _dbFactory.CreateDbContext();
            await db.Database.MigrateAsync(stoppingToken);
            await _busControl.StartAsync(stoppingToken);
        }

        public override void Dispose()
        {
            Task.Run(async () => await _busControl.StopAsync());
        }
    }
}
