using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People;
using Bookstore.Entities.People.AutoMapper;
using Bookstore.Entities.People.Repositories;
using Bookstore.Services.People.CommandHandlers;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using Microsoft.Azure.ServiceBus.Primitives;
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
            var azureConfig = config.GetSection("Azure");
            var storageConfig = config.GetSection("AzureStorage");
            var keyVaultConfig = config.GetSection("KeyVault");
            var certificate =
                new X509Certificate2(azureConfig["CertificatePath"], azureConfig["CertificatePassphrase"]);
            var credential = new ClientCertificateCredential(azureConfig["TenantId"], azureConfig["ApplicationId"], certificate);
            var secretClient = new SecretClient(new Uri(keyVaultConfig["Url"]), credential);
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddDbContextFactory<PeopleContext>(opt =>
            {
                var connectionStringSecret = config.GetConnectionString("PeopleContext");
                var connectionString = secretClient.GetSecret(connectionStringSecret).Value.Value;
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
            MessageDataDefaults.Threshold = 256 * 1024;
            MessageDataDefaults.TimeToLive = TimeSpan.FromDays(2);
            var blobServiceClient = new BlobServiceClient(new Uri($"https://{storageConfig["AccountName"]}.blob.core.windows.net/"), new ManagedIdentityCredential());
            var messageDataRepository = blobServiceClient.CreateMessageDataRepository(storageConfig["MessageDataContainer"]);
            services.AddSingleton<IMessageDataRepository>(messageDataRepository);
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetAssembly(typeof(SaveSubjectCommandHandler)));
                var peopleConfig = config.GetSection("PeopleService");
                var peopleConnection = $"sb://{peopleConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                var secretName = peopleConfig["AccessKeySecret"];
                var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                var hostSettings = new HostSettings
                {
                    ServiceUri = new Uri(peopleConnection),
                    TokenProvider =
                        TokenProvider.CreateSharedAccessSignatureTokenProvider(peopleConfig["AccessKeyName"],
                            sharedAccessKey)
                };
                mt.UsingAzureServiceBus((ctx, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    sb.Host(hostSettings);
                    sb.ReceiveEndpoint(peopleConfig["InputQueue"], rcv =>
                    {
                        rcv.ConfigureConsumers(ctx); 
                    });
                    sb.UseJsonSerializer();
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
