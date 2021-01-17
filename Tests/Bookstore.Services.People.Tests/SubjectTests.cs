using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using NUnit.Framework;
using RabbitWarren;
using RabbitWarren.Messaging;

namespace Bookstore.Services.People.Tests
{
    [TestFixture]
    public class SubjectTests
    {
        private RabbitMQConnection _rmqConnection;
        private RabbitMQPublishChannel _publishChannel;
        private RabbitMQConsumer _consumer;
        private PersonFiller _personFiller;
        private CompanyFiller _companyFiller;

        /* start the people service worker manually first! */
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _personFiller = new PersonFiller();
            _companyFiller = new CompanyFiller();
            var rmqFactory = new RabbitMQConnectionFactory(RabbitMQProtocol.AMQP, "127.0.0.1", "people", 5672, null, new ContainerBuilder().Build(), "brian", "development");
            _rmqConnection = rmqFactory.Create();
            _publishChannel = _rmqConnection.OpenPublishChannel("rabbitwarren");
            var consumerChannel = _rmqConnection.OpenConsumerChannel("", $"response.{Guid.NewGuid()}", autoDelete: true);
            _consumer = consumerChannel.RegisterDefaultConsumer();
            _consumer.Start();
        }

        [Test]
        public async Task TestSaveSubject()
        {
            var person = _personFiller.FillPerson();
            var saveCommand = new SaveSubjectCommand {Subject = person};
            var saveResult = await _publishChannel.Request(saveCommand, "people", _consumer.Channel.Queue);
            if (!(saveResult is SaveSubjectCommandResult personResult)) Assert.Fail("Invalid response received to SaveSubject() request");
            else
            {
                if (personResult.Error != null) Assert.Fail(personResult.Error);
                var subject = personResult.Subject;
                Assert.NotNull(subject);
                Assert.AreNotSame(subject, person);
                Assert.AreEqual(person.Id, subject.Id);
            }
            var company = _companyFiller.FillCompany();
            saveCommand = new SaveSubjectCommand {Subject = company};
            saveResult = await _publishChannel.Request(saveCommand, "people", _consumer.Channel.Queue);
            if (!(saveResult is SaveSubjectCommandResult companyResult)) Assert.Fail("Invalid response received to SaveSubject() request");
            else
            {
                if (companyResult.Error != null) Assert.Fail(companyResult.Error);
                var subject = companyResult.Subject;
                Assert.NotNull(subject);
                Assert.AreNotSame(company, subject);
                Assert.AreEqual(company, subject);
            }
        }

        [Test]
        public async Task TestFindSubject()
        {
            // create a test person and test company
            var person = _personFiller.FillPerson();
            var company = _companyFiller.FillCompany();
            var savePersonCommand = new SaveSubjectCommand {Subject = person};
            var saveCompanyCommand = new SaveSubjectCommand {Subject = company};
            var savePersonTask = _publishChannel.Request(savePersonCommand, "people", _consumer.Channel.Queue);
            var saveCompanyTask = _publishChannel.Request(saveCompanyCommand, "people", _consumer.Channel.Queue);

            // since we can, we'll test parallel commands and queries
            await Task.WhenAll(savePersonTask, saveCompanyTask);

            // verify test data creation
            var savePersonMessage = savePersonTask.Result;
            var saveCompanyMessage = saveCompanyTask.Result;
            if (savePersonMessage is ErrorResult personError)
                Assert.Fail(personError.Error);
            if (savePersonMessage is SaveSubjectCommandResult savePersonResult)
            {
                if (savePersonResult.Error != null) Assert.Fail(savePersonResult.Error);
                if (!savePersonResult.Success) Assert.Fail("Unable to save test person data");
                person = (Person) savePersonResult.Subject;
            }
            if (saveCompanyMessage is ErrorResult companyError)
                Assert.Fail(companyError.Error);
            if (saveCompanyMessage is SaveSubjectCommandResult companyResult)
            {
                if (companyResult.Error != null) Assert.Fail(companyResult.Error);
                if (!companyResult.Success) Assert.Fail("Unable to save test company data");
                company = (Company) companyResult.Subject;
            }

            // verify that we can retrieve both a person and a company, and that retrieving all subjects returns both people and companies
            var findAllQuery = new FindSubjectsQuery();
            var findPersonQuery = new FindSubjectsQuery {SubjectId = person.Id};
            var findCompanyQuery = new FindSubjectsQuery {SubjectId = company.Id};
            var findAllTask = _publishChannel.Request(findAllQuery, "people", _consumer.Channel.Queue);
            var findPersonTask = _publishChannel.Request(findPersonQuery, "people", _consumer.Channel.Queue);
            var findCompanyTask = _publishChannel.Request(findCompanyQuery, "people", _consumer.Channel.Queue);

            // parallel again
            await Task.WhenAll(findAllTask, findPersonTask, findCompanyTask);

            var findAllMessage = findAllTask.Result;
            var findPersonMessage = findPersonTask.Result;
            var findCompanyMessage = findCompanyTask.Result;

            // check for errors
            if (findAllMessage is ErrorResult findAllError)
                Assert.Fail(findAllError.Error);
            if (findPersonMessage is ErrorResult findPersonError)
                Assert.Fail(findPersonError.Error);
            if (findCompanyMessage is ErrorResult findCompanyError)
                Assert.Fail(findCompanyError.Error);

            // verify that find subjects returns both people and companies
            if (findAllMessage is FindSubjectsQueryResult findAllResult)
            {
                if (findAllResult.Error != null)
                    Assert.Fail(findAllResult.Error);
                Assert.IsTrue(findAllResult.Results.Contains(person));
                Assert.IsTrue(findAllResult.Results.Contains(company));
            }

            // verify that if a subject is a person, a Person object is returned
            if (findPersonMessage is FindSubjectsQueryResult findPersonResult)
            {
                if (findPersonResult.Error != null)
                    Assert.Fail(findPersonResult.Error);
                Assert.IsTrue(findPersonResult.Results.Count == 1);
                // AreEqual() will also ensure that the result is of the correct type, since a generic Subject can't be equal to a Person
                Assert.AreEqual(person, findPersonResult.Results.Single());
            }

            // verify that if a subject is a company, a Company object is returned
            if (findCompanyMessage is FindSubjectsQueryResult findCompanyResult)
            {
                if (findCompanyResult.Error != null)
                    Assert.Fail(findCompanyResult.Error);
                Assert.IsTrue(findCompanyResult.Results.Count == 1);
                Assert.AreEqual(company, findCompanyResult.Results.Single());
            }
        }

        [Test]
        public async Task TestRemoveSubject()
        {
            var person = _personFiller.FillPerson();
            var company = _companyFiller.FillCompany();
            var savePersonCommand = new SaveSubjectCommand {Subject = person};
            var saveCompanyCommand = new SaveSubjectCommand {Subject = company};
            var savePersonTask = _publishChannel.Request(savePersonCommand, "people", _consumer.Channel.Queue);
            var saveCompanyTask = _publishChannel.Request(saveCompanyCommand, "people", _consumer.Channel.Queue);
            await Task.WhenAll(savePersonTask, saveCompanyTask);
            var savePersonResponse = savePersonTask.Result;
            var saveCompanyResponse = saveCompanyTask.Result;
            if (savePersonResponse is ErrorResult savePersonError)
                Assert.Fail(savePersonError.Error);
            if (saveCompanyResponse is ErrorResult saveCompanyError)
                Assert.Fail(saveCompanyError.Error);
            if (savePersonResponse is SaveSubjectCommandResult savePersonResult)
                person = savePersonResult.Subject as Person;
            Assert.NotNull(person);
            if (saveCompanyResponse is SaveSubjectCommandResult saveCompanyResult)
                company = saveCompanyResult.Subject as Company;
            Assert.NotNull(company);
            var removePersonCommand = new RemoveSubjectCommand {SubjectId = person.Id};
            var removeCompanyCommand = new RemoveSubjectCommand {SubjectId = company.Id};
            var removePersonTask = _publishChannel.Request(removePersonCommand, "people", _consumer.Channel.Queue);
            var removeCompanyTask = _publishChannel.Request(removeCompanyCommand, "people", _consumer.Channel.Queue);
            await Task.WhenAll(removePersonTask, removeCompanyTask);
            var removePersonResponse = removePersonTask.Result;
            var removeCompanyResponse = removeCompanyTask.Result;
            switch (removeCompanyResponse)
            {
                case ErrorResult removeCompanyError:
                    Assert.Fail(removeCompanyError.Error);
                    break;
                case RemoveSubjectCommandResult removeCompanyResult:
                    Assert.IsTrue(removeCompanyResult.Success);
                    break;
                default:
                    Assert.Fail("Unknown response type received to remove company command");
                    break;
            }
            switch (removePersonResponse)
            {
                case ErrorResult removePersonError:
                    Assert.Fail(removePersonError.Error);
                    break;
                case RemoveSubjectCommandResult removePersonResult:
                    Assert.IsTrue(removePersonResult.Success);
                    break;
                default:
                    Assert.Fail("Unknown response type received to remove person command");
                    break;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _rmqConnection.Close();
        }
    }
}
