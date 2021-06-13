using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using Bookstore.ObjectFillers;
using MassTransit;
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
        
        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddMassTransit(mt =>
            {
                mt.AddRequestClient<SaveSubjectCommand>();
                mt.AddRequestClient<FindSubjectsQuery>();
                mt.AddRequestClient<RemoveSubjectCommand>();
                mt.UsingRabbitMq((_, rmq) =>
                {
                    var connectionString = config.GetConnectionString("PeopleService");
                    rmq.Host(new Uri(connectionString));
                    rmq.UseBsonSerializer();
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
            var foundPerson = foundPersonResponse.Message.Results.SingleOrDefault();
            var foundOrganization = foundOrganizationResponse.Message.Results.SingleOrDefault();
            var allSubjectsResponse = await _findSubjectsQuery.GetResponse<FindSubjectsQueryResult>(
                new FindSubjectsQuery());
            Assert.NotNull(foundPerson);
            Assert.NotNull(foundOrganization);
            Assert.AreEqual(person, foundPerson);
            Assert.AreEqual(organization, foundOrganization);
            Assert.IsTrue(allSubjectsResponse.Message.Results.Any(r => r.Id == person.Id));
            Assert.IsTrue(allSubjectsResponse.Message.Results.Contains(person));
            Assert.IsTrue(allSubjectsResponse.Message.Results.Any(r => r.Id == organization.Id));
            Assert.IsTrue(allSubjectsResponse.Message.Results.Contains(organization));
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
            var foundPerson = foundPersonResponse.Message.Results.SingleOrDefault();
            var foundOrganization = foundOrganizationResponse.Message.Results.SingleOrDefault();
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
