using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
using Bookstore.Services.People.CommandHandlers;
using MassTransit;
using MassTransit.Serialization;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bookstore.Services.Workers.People
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static void BuildServiceContainer(IServiceCollection services)
        {
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionString = "server=mysql;user=brian;password=development;database=PeopleDevelopment";
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICountryRepository, AddressRepository>();
            services.AddScoped<IProvinceRepository, AddressRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetAssembly(typeof(SaveSubjectCommandHandler)));
                mt.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(new Uri("amqp://rabbitmq:5672/people"), host =>
                    {
                        host.Username("brian");
                        host.Password("development");
                    });
                    cfg.UseBsonSerializer();
                    cfg.ConfigureEndpoints(ctx);
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
