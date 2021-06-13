using System;
using System.Reflection;
using AutoMapper;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Queries;
using Bookstore.Entities.Book;
using Bookstore.Entities.Book.AutoMapper;
using Bookstore.Entities.Book.Repositories;
using Bookstore.Services.Book.CommandHandlers;
using MassTransit;
using MassTransit.MultiBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        private static void BuildServiceContainer(IServiceCollection services, IConfiguration config)
        {
            services.AddDbContextFactory<BookContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = config.GetConnectionString("BookContext");
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IPublisherRepository, PublisherRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            services.AddSingleton(mapperConfig.CreateMapper());
            services.AddMassTransit(cfg =>
            {
                var bookServiceConnection = config.GetConnectionString("BookService");
                cfg.AddConsumers(Assembly.GetAssembly(typeof(SaveBookCommandHandler)));
                cfg.UsingRabbitMq((ctx, rmq) =>
                {
                    rmq.Host(new Uri(bookServiceConnection));
                    rmq.UseBsonSerializer();
                    rmq.ConfigureEndpoints(ctx);
                });
            });

            services.AddMassTransit<IPeopleBus>(cfg =>
            {
                cfg.AddRequestClient<SaveSubjectCommand>();
                cfg.AddRequestClient<RemoveSubjectCommand>();
                cfg.AddRequestClient<FindSubjectsQuery>();
                var peopleServiceConnection = config.GetConnectionString("PeopleService");
                cfg.UsingRabbitMq((_, rmq) =>
                {
                    rmq.Host(new Uri(peopleServiceConnection));
                    rmq.UseBsonSerializer();
                }); 
            });
            services.AddMassTransitHostedService();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    BuildServiceContainer(services, hostContext.Configuration);
                    services.AddHostedService<Worker>();
                    services.AddLogging(cfg => cfg.AddConsole());
                });
    }
}