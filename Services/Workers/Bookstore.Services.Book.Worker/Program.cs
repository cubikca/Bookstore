using System;
using System.Reflection;
using AutoMapper;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Entities.Book;
using Bookstore.Entities.Book.AutoMapper;
using Bookstore.Entities.Book.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.Book.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static void BuildServiceContainer(IServiceCollection services)
        {
            services.AddDbContextFactory<BookContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer("Data Source=sqlserver;Initial Catalog=BooksDevelopment;User Id=brian;Password=development");
            });
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IPublisherRepository, PublisherRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<BookProfile>();
                cfg.AddProfile<PublisherProfile>();
            });
            services.AddSingleton(mapperConfig.CreateMapper());
            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(Assembly.GetAssembly(typeof(SaveBookCommand)));
                cfg.UsingRabbitMq((ctx, rmq) =>
                {
                    rmq.Host(new Uri("amqp://rabbitmq:5672/books"), host =>
                    {
                        host.Username("brian");
                        host.Password("development");
                    });
                    rmq.UseBsonSerializer();
                    rmq.ConfigureEndpoints(ctx);
                });
            });
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    BuildServiceContainer(services);
                    services.AddHostedService<Worker>();
                    services.AddLogging(cfg => cfg.AddConsole());
                });
    }
}