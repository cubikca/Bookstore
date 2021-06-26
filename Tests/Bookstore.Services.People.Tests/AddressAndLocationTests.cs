using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.ObjectFillers;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bookstore.Services.People.Tests
{
    public class AddressAndLocationTests
    {
        private IServiceProvider _services;
        private IBusControl _busControl;
        private IRequestClient<SaveAddressCommand> _saveAddressCommand;
        private IRequestClient<SaveLocationCommand> _saveLocationCommand;
        private IRequestClient<FindAddressesQuery> _findAddressesQuery;
        private IRequestClient<FindLocationsQuery> _findLocationsQuery;
        private IRequestClient<RemoveAddressCommand> _removeAddressCommand;
        private IRequestClient<RemoveLocationCommand> _removeLocationCommand;
        private AddressFiller _addressFiller;
        private LocationFiller _locationFiller;
        
        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var azureConfig = config.GetSection("Azure");
            var certificate = new X509Certificate2(azureConfig["CertificatePath"], azureConfig["CertificatePassphrase"]);
            var credential = new ClientCertificateCredential(azureConfig["TenantId"], azureConfig["ApplicationId"], certificate);
            services.AddSingleton(credential);
            var keyVaultConfig = config.GetSection("KeyVault");
            var secretClient = new SecretClient(new Uri(keyVaultConfig["Url"]), credential, new SecretClientOptions());
            services.AddSingleton(secretClient);
            var storageConfig = config.GetSection("AzureStorage");
            var blobServiceClient =
                new BlobServiceClient(new Uri($"https://{storageConfig["AccountName"]}.blob.core.windows.net"),
                    new ManagedIdentityCredential());
            var messageDataRepository = blobServiceClient.CreateMessageDataRepository(storageConfig["MessageDataContainer"]);
            services.AddSingleton<IMessageDataRepository>(messageDataRepository);
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddMassTransit(mt =>
            {
                mt.AddRequestClient<SaveAddressCommand>();
                mt.AddRequestClient<SaveLocationCommand>();
                mt.AddRequestClient<FindAddressesQuery>();
                mt.AddRequestClient<FindLocationsQuery>();
                mt.AddRequestClient<RemoveAddressCommand>();
                mt.AddRequestClient<RemoveLocationCommand>();
                mt.UsingAzureServiceBus((ctx, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var peopleConfig = config.GetSection("PeopleService");
                    var peopleConnection = $"sb://{peopleConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                    var secretName = peopleConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(peopleConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(peopleConfig["AccessKeyName"],
                            sharedAccessKey)
                    };
                    sb.Host(hostSettings);
                    sb.UseJsonSerializer();
                });
            });
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build();
            var services = new ServiceCollection();
            ConfigureServices(services, config);
            _services = services.BuildServiceProvider();
            _busControl = _services.GetRequiredService<IBusControl>();
            _saveAddressCommand = _services.GetRequiredService<IRequestClient<SaveAddressCommand>>();
            _saveLocationCommand = _services.GetRequiredService<IRequestClient<SaveLocationCommand>>();
            _findAddressesQuery = _services.GetRequiredService<IRequestClient<FindAddressesQuery>>();
            _findLocationsQuery = _services.GetRequiredService<IRequestClient<FindLocationsQuery>>();
            _removeAddressCommand = _services.GetRequiredService<IRequestClient<RemoveAddressCommand>>();
            _removeLocationCommand = _services.GetRequiredService<IRequestClient<RemoveLocationCommand>>();
            _addressFiller = new AddressFiller();
            _locationFiller = new LocationFiller();
            _busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var location = _locationFiller.FillLocation();
            var createdLocationResponse = await _saveLocationCommand.GetResponse<SaveLocationCommandResult>(
                new SaveLocationCommand {Location = location});
            Assert.NotNull(createdLocationResponse.Message.Location);
            Assert.AreNotSame(location, createdLocationResponse.Message.Location);
            Assert.AreEqual(location.Id, createdLocationResponse.Message.Location.Id);
            Assert.AreEqual(location, createdLocationResponse.Message.Location);
            var streetAddress = _addressFiller.FillAddress();
            var mailingAddress = _addressFiller.FillAddress();
            var createdAddressResponse1 = await _saveAddressCommand.GetResponse<SaveAddressCommandResult>(
                new SaveAddressCommand {Address = streetAddress});
            var createdAddressResponse2 = await _saveAddressCommand.GetResponse<SaveAddressCommandResult>(
                new SaveAddressCommand {Address = mailingAddress});
            Assert.NotNull(createdAddressResponse1.Message.Address);
            Assert.NotNull(createdAddressResponse2.Message.Address);
            Assert.AreEqual(streetAddress.Id, createdAddressResponse1.Message.Address.Id);
            Assert.AreEqual(mailingAddress.Id, createdAddressResponse2.Message.Address.Id);
            Assert.AreEqual(streetAddress, createdAddressResponse1.Message.Address);
            Assert.AreEqual(mailingAddress, createdAddressResponse2.Message.Address);
            var updatedLocation = _locationFiller.FillLocation();
            updatedLocation.Id = location.Id;
            updatedLocation.StreetAddress = streetAddress;
            updatedLocation.MailingAddress = mailingAddress;
            var updatedLocationResponse = await _saveLocationCommand.GetResponse<SaveLocationCommandResult>(
                new SaveLocationCommand {Location = updatedLocation});
            var findLocationResponse = await _findLocationsQuery.GetResponse<FindLocationsQueryResult>(
                new FindLocationsQuery {LocationId = updatedLocation.Id});
            var locationsJson = await findLocationResponse.Message.Results.Value;
            var locations = JsonConvert.DeserializeObject<List<Location>>(locationsJson);
            var foundLocation = locations.SingleOrDefault();
            Assert.NotNull(foundLocation);
            // this is really the best we can do. the address updates were saved through a different repository
            // and so our local object is out of sync with the database. Only Find() will give us an object for comparison
            Assert.AreEqual(foundLocation.Id, updatedLocationResponse.Message.Location.Id);
            Assert.AreEqual(foundLocation, updatedLocationResponse.Message.Location);
            var updatedAddress = _addressFiller.FillAddress();
            updatedAddress.Id = mailingAddress.Id;
            var updatedAddressResponse = await _saveAddressCommand.GetResponse<SaveAddressCommandResult>(
                new SaveAddressCommand {Address = updatedAddress});
            Assert.AreEqual(mailingAddress.Id, updatedAddressResponse.Message.Address.Id);
            Assert.AreEqual(updatedAddress, updatedAddressResponse.Message.Address);
        }

        [Test]
        public async Task TestFind()
        {
            var location = _locationFiller.FillLocation();
            await _saveLocationCommand.GetResponse<SaveLocationCommandResult>(
                new SaveLocationCommand {Location = location});
            var foundLocationResponse = await _findLocationsQuery.GetResponse<FindLocationsQueryResult>(
                new FindLocationsQuery {LocationId = location.Id});
            var foundMailingAddressResponse = await _findAddressesQuery.GetResponse<FindAddressesQueryResult>(
                new FindAddressesQuery {AddressId = location.MailingAddress.Id});
            var foundStreetAddressResponse = await _findAddressesQuery.GetResponse<FindAddressesQueryResult>(
                new FindAddressesQuery {AddressId = location.StreetAddress.Id});
            var locationsJson = await foundLocationResponse.Message.Results.Value;
            var locations = JsonConvert.DeserializeObject<List<Location>>(locationsJson);
            var foundLocation = locations.SingleOrDefault();
            var addressJson1 = await foundMailingAddressResponse.Message.Results.Value;
            var addresses1 = JsonConvert.DeserializeObject<List<Address>>(addressJson1);
            var foundMailingAddress = addresses1.SingleOrDefault();
            var addressJson2 = await foundStreetAddressResponse.Message.Results.Value;
            var addresses2 = JsonConvert.DeserializeObject<List<Address>>(addressJson2);
            var foundStreetAddress = addresses2.SingleOrDefault();
            Assert.NotNull(foundLocation);
            Assert.NotNull(foundMailingAddress);
            Assert.NotNull(foundStreetAddress);
            Assert.AreEqual(location.Id, foundLocation.Id);
            Assert.AreEqual(location, foundLocation);
            Assert.AreEqual(location.MailingAddress.Id, foundMailingAddress.Id);
            Assert.AreEqual(location.MailingAddress, foundMailingAddress);
            Assert.AreEqual(location.StreetAddress.Id, foundStreetAddress.Id);
            Assert.AreEqual(location.StreetAddress, foundStreetAddress);
        }

        [Test]
        public async Task TestRemove()
        {
            var location = _locationFiller.FillLocation();
            await _saveLocationCommand.GetResponse<SaveLocationCommandResult>(
                new SaveLocationCommand {Location = location});
            var removeStreetAddressResponse = await _removeAddressCommand.GetResponse<RemoveAddressCommandResult>(
                new RemoveAddressCommand {AddressId = location.StreetAddress.Id});
            var foundLocationResponse = await _findLocationsQuery.GetResponse<FindLocationsQueryResult>(
                new FindLocationsQuery {LocationId = location.Id});
            var locationsJson = await foundLocationResponse.Message.Results.Value;
            var locations = JsonConvert.DeserializeObject<List<Location>>(locationsJson);
            location = locations.SingleOrDefault();
            Assert.NotNull(location);
            Assert.IsTrue(removeStreetAddressResponse.Message.Success);
            Assert.IsNull(location.StreetAddress);
            var removeLocationResponse = await _removeLocationCommand.GetResponse<RemoveLocationCommandResult>(
                new RemoveLocationCommand {LocationId = location.Id});
            Assert.IsTrue(removeLocationResponse.Message.Success);
            foundLocationResponse = await _findLocationsQuery.GetResponse<FindLocationsQueryResult>(
                new FindLocationsQuery {LocationId = location.Id});
            var foundMailingAddressResponse = await _findAddressesQuery.GetResponse<FindAddressesQueryResult>(
                new FindAddressesQuery {AddressId = location.MailingAddress.Id});
            locationsJson = await foundLocationResponse.Message.Results.Value;
            locations = JsonConvert.DeserializeObject<List<Location>>(locationsJson);
            var foundLocation = locations.SingleOrDefault();
            var addressesJson = await foundMailingAddressResponse.Message.Results.Value;
            var addresses = JsonConvert.DeserializeObject<List<Address>>(addressesJson);
            var foundMailingAddress = addresses.SingleOrDefault();
            Assert.IsNull(foundMailingAddress);
            Assert.IsNull(foundLocation);
        }
    }
}