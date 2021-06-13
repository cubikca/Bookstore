using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Bookstore.Services.People.CommandHandlers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookstore.Services.Workers.People
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static void BuildServiceContainer(IServiceCollection services, IConfiguration config)
        {
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                var connectionString = config.GetConnectionString("PeopleContext");
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DefaultProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetAssembly(typeof(SaveSubjectCommandHandler)));
                var peopleConnection = config.GetConnectionString("PeopleService");
                mt.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(new Uri(peopleConnection));
                    cfg.UseBsonSerializer();
                    cfg.ConfigureEndpoints(ctx);
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
                });
    }
}
