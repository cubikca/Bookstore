using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Bookstore.Domains.Book;
using Bookstore.Domains.Book.Commands;
using Bookstore.Domains.Book.QueryResults;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Queries;
using Bookstore.Entities.Book;
using Bookstore.Entities.Book.AutoMapper;
using Bookstore.Entities.Book.Repositories;
using Bookstore.Services.Book.CommandHandlers;
using Enchilada.Azure.BlobStorage;
using Enchilada.Filesystem;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using MassTransit.MessageData.Enchilada;
using MassTransit.MultiBus;
using Microsoft.Azure.ServiceBus.Primitives;
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
            var azureConfig = config.GetSection("Azure");
            var storageConfig = config.GetSection("AzureStorage");
            var keyVaultConfig = config.GetSection("KeyVault");
            var certificate =
                new X509Certificate2(azureConfig["CertificatePath"], azureConfig["CertificatePassphrase"]);
            var credential = new ClientCertificateCredential(azureConfig["TenantId"], azureConfig["ApplicationId"], certificate);
            var secretClient = new SecretClient(new Uri(keyVaultConfig["Url"]), credential);
            services.AddDbContextFactory<BookContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                var connectionStringSecret = config.GetConnectionString("BookContext");
                var connectionString = secretClient.GetSecret(connectionStringSecret).Value.Value;
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
            MessageDataDefaults.TimeToLive = TimeSpan.FromDays(2);
            var blobServiceClient = new BlobServiceClient(new Uri($"https://{storageConfig["AccountName"]}.blob.core.windows.net/"), new ManagedIdentityCredential());
            var messageDataRepository = blobServiceClient.CreateMessageDataRepository(storageConfig["MessageDataContainer"]);
            services.AddSingleton<IMessageDataRepository>(messageDataRepository);
            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumers(Assembly.GetAssembly(typeof(SaveBookCommandHandler)));
                cfg.UsingAzureServiceBus((ctx, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var booksConfig = config.GetSection("BooksService");
                    var booksConnection = $"sb://{booksConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                    var secretName = booksConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(booksConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(booksConfig["AccessKeyName"],
                            sharedAccessKey)
                    };
                    sb.Host(hostSettings);
                    sb.ReceiveEndpoint(booksConfig["InputQueue"], ep =>
                    {
                        ep.ConfigureConsumers(ctx);
                    }); 
                    sb.Host(hostSettings);
                    sb.UseJsonSerializer();
                });
            });
            services.AddMassTransit<IPeopleBus>(cfg =>
            {
                cfg.AddRequestClient<SaveSubjectCommand>();
                cfg.AddRequestClient<RemoveSubjectCommand>();
                cfg.AddRequestClient<FindSubjectsQuery>();
                cfg.UsingAzureServiceBus((_, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var peopleConfig = config.GetSection("PeopleService");
                    var peopleConnection = $"sb://{peopleConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                    var secretName = peopleConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(peopleConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                            peopleConfig["AccessKeyName"], sharedAccessKey)
                    };
                    sb.Host(hostSettings);
                    sb.UseBsonSerializer();
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