using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitWarren;

namespace Bookstore.Services.Workers.People
{
    public class Program
    {
        private static RabbitMQConnectionFactory RabbitMQConnectionFactory;

        public static async Task Main(string[] args)
        {
            RabbitMQConnectionFactory = new RabbitMQConnectionFactory(RabbitMQProtocol.AMQP, "localhost", "people", 5672, null, null, "brian", "development");
            RabbitMQConnectionFactory.ServiceContainer = BuildServiceContainer();
            var dbFactory = RabbitMQConnectionFactory.ServiceContainer.Resolve<IDbContextFactory<PeopleContext>>();
            await using var db = dbFactory.CreateDbContext();
            await db.Database.MigrateAsync();
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IContainer BuildServiceContainer()
        {
            var builder = new ContainerBuilder();
            var services = new ServiceCollection();
            // any dependencies for mediatr handlers (e.g. repositories, dbcontext, automapper) go here
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlServer("Data Source=localhost;User Id=brian;Password=development;Initial Catalog=PeopleDevelopment");
            });
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddSingleton(RabbitMQConnectionFactory);
            services.AddSingleton(new RabbitMQOptions
            {
                Exchange = "rabbitwarren",
                EventQueue = "people",
                Host = "localhost",
                Password = "development",
                Port = 5672,
                Username = "brian",
                VirtualHost = "people"
            });
            services.AddScoped(sp => sp.GetService<RabbitMQConnectionFactory>()?.Create());
            services.AddLogging(cfg => cfg.AddConsole());
            builder.Populate(services);
            var mediatrTypes = new[] {typeof(IRequestHandler<,>)};
            foreach (var type in mediatrTypes)
            {
                builder.RegisterAssemblyTypes(typeof(SaveSubjectCommand).Assembly)
                    .AsClosedTypesOf(type)
                    .InstancePerDependency();
            }
            return builder.Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    var rmqFactory = new RabbitMQConnectionFactory(RabbitMQProtocol.AMQP, "localhost", "people", 5672, null, BuildServiceContainer(), "brian", "development");
                    services.AddSingleton(rmqFactory);
                    services.AddLogging(cfg => cfg.AddConsole());
                    services.AddTransient(sp =>
                    {
                        var factory = sp.GetService<RabbitMQConnectionFactory>();
                        var conn = factory?.Create();
                        return conn;
                    });
                });
    }
}
