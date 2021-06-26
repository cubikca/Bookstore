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
using Bookstore.ObjectFillers;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Configurators;
using MassTransit.MessageData;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bookstore.Services.People.Tests
{
    public class SubjectTests
    {
        private IServiceProvider _services;
        private IRequestClient<SaveSubjectCommand> _saveSubjectCommand;
        private IRequestClient<FindSubjectsQuery> _findSubjectsQuery;
        private IRequestClient<RemoveSubjectCommand> _removeSubjectCommand;
        private PersonFiller _personFiller;
        private OrganizationFiller _organizationFiller;
        
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
                mt.AddRequestClient<SaveSubjectCommand>();
                mt.AddRequestClient<FindSubjectsQuery>();
                mt.AddRequestClient<RemoveSubjectCommand>();
                mt.UsingAzureServiceBus((ctx, sb) =>
                {
                    sb.UseMessageData(messageDataRepository);
                    var peopleConfig = config.GetSection("PeopleService");
                    var secretName = peopleConfig["AccessKeySecret"];
                    var sharedAccessKey = secretClient.GetSecret(secretName).Value.Value;
                    var peopleConnection = $"sb://{peopleConfig["ServiceBusNamespace"]}.servicebus.windows.net/";
                    var hostSettings = new HostSettings
                    {
                        ServiceUri = new Uri(peopleConnection),
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(peopleConfig["AccessKeyName"], sharedAccessKey)
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
            _saveSubjectCommand = _services.GetRequiredService<IRequestClient<SaveSubjectCommand>>();
            _findSubjectsQuery = _services.GetRequiredService<IRequestClient<FindSubjectsQuery>>();
            _removeSubjectCommand = _services.GetRequiredService<IRequestClient<RemoveSubjectCommand>>();
            _personFiller = new PersonFiller();
            _organizationFiller = new OrganizationFiller();
            var busControl = _services.GetRequiredService<IBusControl>();
            busControl.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var person = _personFiller.FillPerson();
            var organization = _organizationFiller.FillOrganization();
            var savePersonResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = person});
            var saveOrganizationResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = organization});
            Assert.NotNull(savePersonResponse.Message.Subject);
            Assert.NotNull(saveOrganizationResponse.Message.Subject);
            Assert.AreEqual(person.Id, savePersonResponse.Message.Subject.Id);
            Assert.AreEqual(organization.Id, saveOrganizationResponse.Message.Subject.Id);
            Assert.AreEqual(person, savePersonResponse.Message.Subject);
            Assert.AreEqual(organization, saveOrganizationResponse.Message.Subject);
            person = _personFiller.FillPerson();
            organization = _organizationFiller.FillOrganization();
            person.Id = savePersonResponse.Message.Subject.Id;
            organization.Id = saveOrganizationResponse.Message.Subject.Id;
            var updatePersonResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = person});
            var updateOrganizationResponse = await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = organization});
            Assert.NotNull(updatePersonResponse.Message.Subject);
            Assert.NotNull(updateOrganizationResponse.Message.Subject);
            Assert.AreEqual(person.Id, updatePersonResponse.Message.Subject.Id);
            Assert.AreEqual(organization.Id, updateOrganizationResponse.Message.Subject.Id);
            Assert.AreEqual(person, updatePersonResponse.Message.Subject);
            Assert.AreEqual(organization, updateOrganizationResponse.Message.Subject);
        }

        [Test]
        public async Task TestFind()
        {
            var person = _personFiller.FillPerson();
            var organization = _organizationFiller.FillOrganization();
            await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = person});
            await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = organization});
            var foundPersonResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery {SubjectId = person.Id});
            var foundOrganizationResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery {SubjectId = organization.Id});
            var peopleJson = await foundPersonResponse.Message.Results.Value;
            var people = JsonConvert.DeserializeObject<List<Person>>(peopleJson);
            var foundPerson = people.SingleOrDefault();
            var organizationsJson = await foundOrganizationResponse.Message.Results.Value;
            var organizations = JsonConvert.DeserializeObject<List<Organization>>(organizationsJson);
            var foundOrganization = organizations.SingleOrDefault();
            var allSubjectsResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery());
            var subjectsJson = await allSubjectsResponse.Message.Results.Value;
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects })
                ?? Enumerable.Empty<Subject>().ToList();
            Assert.NotNull(foundPerson);
            Assert.NotNull(foundOrganization);
            Assert.AreEqual(person, foundPerson);
            Assert.AreEqual(organization, foundOrganization);
            Assert.IsTrue(subjects.Any(r => r.Id == person.Id));
            Assert.IsTrue(subjects.Contains(person));
            Assert.IsTrue(subjects.Any(r => r.Id == organization.Id));
            Assert.IsTrue(subjects.Contains(organization));
        }

        [Test]
        public async Task TestRemove()
        {
            var person = _personFiller.FillPerson();
            var organization = _organizationFiller.FillOrganization();
            await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = person});
            await _saveSubjectCommand.GetResponse<SaveSubjectCommandResult>(
                new SaveSubjectCommand {Subject = organization});
            var removePersonResponse = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                new RemoveSubjectCommand {SubjectId = person.Id});
            var removeOrganizationResponse = await _removeSubjectCommand.GetResponse<RemoveSubjectCommandResult>(
                new RemoveSubjectCommand {SubjectId = organization.Id});
            Assert.IsTrue(removePersonResponse.Message.Success);
            Assert.IsTrue(removeOrganizationResponse.Message.Success);
            var foundPersonResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery {SubjectId = person.Id});
            var foundOrganizationResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery {SubjectId = organization.Id});
            var peopleJson = await foundPersonResponse.Message.Results.Value;
            var people = JsonConvert.DeserializeObject<List<Person>>(peopleJson);
            var foundPerson = people.SingleOrDefault();
            var organizationsJson = await foundOrganizationResponse.Message.Results.Value;
            var organizations = JsonConvert.DeserializeObject<List<Organization>>(organizationsJson);
            var foundOrganization = organizations.SingleOrDefault();
            Assert.IsNull(foundPerson);
            Assert.IsNull(foundOrganization);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var busControl = _services.GetService<IBusControl>();
            busControl?.Stop();
        }
    }
}
