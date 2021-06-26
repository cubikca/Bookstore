using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.Domains.People.Repositories;
using Bookstore.ObjectFillers;
using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public class CountryAndProvinceTests
    {
        private IServiceProvider _services;
        private IBusControl _busControl;
        private IRequestClient<RemoveCountryCommand> _removeCountryCommand;
        private IRequestClient<RemoveProvinceCommand> _removeProvinceCommand;
        private IRequestClient<SaveCountryCommand> _saveCountryCommand;
        private IRequestClient<SaveProvinceCommand> _saveProvinceCommand;
        private IRequestClient<FindCountriesQuery> _findCountriesQuery;
        private IRequestClient<FindProvincesQuery> _findProvincesQuery;
        private CountryFiller _countryFiller;
        private ProvinceFiller _provinceFiller;
        
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
            services.AddLogging(log => log.AddConsole());
            services.AddMassTransit(mt =>
            {
                mt.AddRequestClient<RemoveCountryCommand>();
                mt.AddRequestClient<RemoveProvinceCommand>();
                mt.AddRequestClient<SaveCountryCommand>();
                mt.AddRequestClient<SaveProvinceCommand>();
                mt.AddRequestClient<FindCountriesQuery>();
                mt.AddRequestClient<FindProvincesQuery>();
                mt.UsingAzureServiceBus((_, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var peopleServiceConfig = config.GetSection("PeopleService");
                    var secretName = peopleServiceConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri($"sb://{peopleServiceConfig["ServiceBusNamespace"]}.servicebus.windows.net/"),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(peopleServiceConfig["AccessKeyName"],
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
            _findCountriesQuery = _services.GetRequiredService<IRequestClient<FindCountriesQuery>>();
            _findProvincesQuery = _services.GetRequiredService<IRequestClient<FindProvincesQuery>>();
            _removeCountryCommand = _services.GetRequiredService<IRequestClient<RemoveCountryCommand>>();
            _removeProvinceCommand = _services.GetRequiredService<IRequestClient<RemoveProvinceCommand>>();
            _saveCountryCommand = _services.GetRequiredService<IRequestClient<SaveCountryCommand>>();
            _saveProvinceCommand = _services.GetRequiredService<IRequestClient<SaveProvinceCommand>>();
            _busControl = _services.GetRequiredService<IBusControl>();
            _busControl.Start();
            _provinceFiller = new ProvinceFiller();
            _countryFiller = new CountryFiller();
        }

        [Test]
        public async Task TestSave()
        {
            var province = _provinceFiller.FillProvince();
            var country = _countryFiller.FillCountry();
            province.Country = country;
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province});
            // now both the province and country should be saved
            var createdProvinceResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {ProvinceId = province.Id});
            var createdCountryResponse = await _findCountriesQuery.GetResponse<FindCountriesQueryResult>(
                new FindCountriesQuery {CountryId = country.Id});
            var provincesJson = await createdProvinceResponse.Message.Results.Value;
            var provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            var createdProvince = provinces.SingleOrDefault();
            var countriesJson = await createdCountryResponse.Message.Results.Value;
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            var createdCountry = countries.SingleOrDefault();
            Assert.IsNotNull(createdProvince);
            Assert.IsNotNull(createdCountry);
            Assert.AreNotSame(province, createdProvince);
            Assert.AreNotSame(country, createdCountry);
            Assert.AreEqual(province, createdProvince);
            Assert.AreEqual(country, createdCountry);
            province = _provinceFiller.FillProvince();
            province.Id = createdProvince.Id;
            country = _countryFiller.FillCountry();
            country.Id = createdCountry.Id;
            province.Country = country;
            await _saveCountryCommand.GetResponse<SaveCountryCommandResult>(
                new SaveCountryCommand {Country = country});
            var updatedCountryResponse = await _findCountriesQuery.GetResponse<FindCountriesQueryResult>(
                new FindCountriesQuery {CountryId = country.Id});
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province});
            var updatedProvinceResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {ProvinceId = province.Id});
            provincesJson = await updatedProvinceResponse.Message.Results.Value;
            provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            var updatedProvince = provinces.SingleOrDefault();
            countriesJson = await updatedCountryResponse.Message.Results.Value;
            countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            var updatedCountry = countries.SingleOrDefault();
            Assert.NotNull(updatedProvince);
            Assert.NotNull(updatedCountry);
            Assert.AreNotSame(province, updatedProvince);
            Assert.AreNotSame(country, updatedCountry);
            Assert.AreEqual(province, updatedProvince);
            Assert.AreEqual(country, updatedCountry);
        }

        [Test]
        public async Task TestFind()
        {
            var province1 = _provinceFiller.FillProvince();
            var province2 = _provinceFiller.FillProvince();
            var province3 = _provinceFiller.FillProvince();
            province2.Country = province1.Country;
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province1});
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province2});
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province3});
            var findSingleProvinceResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {ProvinceId = province1.Id});
            var findCountryProvincesResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {CountryId = province1.Country.Id});
            var findAllProvincesResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery());
            var findSingleCountryResponse = await _findCountriesQuery.GetResponse<FindCountriesQueryResult>(
                new FindCountriesQuery {CountryId = province1.Country.Id});
            var findAllCountriesResponse = await _findCountriesQuery.GetResponse<FindCountriesQueryResult>(
                new FindCountriesQuery());
            var provincesJson = await findSingleProvinceResponse.Message.Results.Value;
            var provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            var foundSingleProvince = provinces.SingleOrDefault();
            var countriesJson = await findSingleCountryResponse.Message.Results.Value;
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            var foundSingleCountry = countries.SingleOrDefault();
            Assert.NotNull(foundSingleProvince);
            Assert.NotNull(foundSingleCountry);
            Assert.AreNotSame(province1, foundSingleProvince);
            Assert.AreEqual(province1, foundSingleProvince);
            provincesJson = await findCountryProvincesResponse.Message.Results.Value;
            provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            Assert.IsTrue(new HashSet<Province> { province1, province2 }.SetEquals(provinces.ToHashSet()));
            provincesJson = await findAllProvincesResponse.Message.Results.Value;
            provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            Assert.IsTrue(new HashSet<Province> { province1, province2, province3 }.All(provinces.Contains));
            // province1.Country == province2.Country
            countriesJson = await findAllCountriesResponse.Message.Results.Value;
            countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            Assert.IsTrue(new HashSet<Country> {province1.Country, province3.Country}.All(countries.Contains));
        }

        [Test]
        public async Task TestRemove()
        {
            var province1 = _provinceFiller.FillProvince();
            var province2 = _provinceFiller.FillProvince();
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province1});
            await _saveProvinceCommand.GetResponse<SaveProvinceCommandResult>(
                new SaveProvinceCommand {Province = province2});
            var removedProvinceResponse = await _removeProvinceCommand.GetResponse<RemoveProvinceCommandResult>(
                new RemoveProvinceCommand {ProvinceId = province1.Id});
            var foundProvinceResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {ProvinceId = province1.Id});
            var provincesJson = await foundProvinceResponse.Message.Results.Value;
            var provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            Assert.IsTrue(removedProvinceResponse.Message.Success);
            Assert.IsFalse(provinces.Any());
            var removedCountryResponse = await _removeCountryCommand.GetResponse<RemoveCountryCommandResult>(
                new RemoveCountryCommand {CountryId = province2.Country.Id});
            var foundCountryResponse = await _findCountriesQuery.GetResponse<FindCountriesQueryResult>(
                new FindCountriesQuery {CountryId = province2.Country.Id});
            foundProvinceResponse = await _findProvincesQuery.GetResponse<FindProvincesQueryResult>(
                new FindProvincesQuery {ProvinceId = province2.Id});
            var countriesJson = await foundCountryResponse.Message.Results.Value;
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            Assert.IsTrue(removedCountryResponse.Message.Success);
            Assert.IsFalse(countries.Any());
            provincesJson = await foundProvinceResponse.Message.Results.Value;
            provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
            Assert.IsFalse(provinces.Any());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _busControl.Stop();
        }
    }
}
