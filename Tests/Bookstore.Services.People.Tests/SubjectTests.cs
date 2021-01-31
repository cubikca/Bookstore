using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.CommandResults;
using Bookstore.Domains.People.Commands;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Queries;
using Bookstore.Domains.People.QueryResults;
using MassTransit;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework;

namespace Bookstore.Services.People.Tests
{
    [TestFixture]
    public class SubjectTests
    {
        private IBusControl _busControl;
        private IRequestClient<SaveSubjectCommand> _saveSubjectClient;
        private IRequestClient<FindSubjectsQuery> _findSubjectsClient;
        private IRequestClient<RemoveSubjectCommand> _removeSubjectClient;

        private PersonFiller _personFiller;
        private CompanyFiller _companyFiller;

        /* start the people service worker manually first! */
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _personFiller = new PersonFiller();
            _companyFiller = new CompanyFiller();
            _busControl = Bus.Factory.CreateUsingRabbitMq(rmq =>
            {
                rmq.Host(new Uri("amqp://localhost:5672/people"), host =>
                {
                    host.Username("brian");
                    host.Password("development");
                });
            });
            await _busControl.StartAsync();
            _saveSubjectClient = _busControl.CreateRequestClient<SaveSubjectCommand>();
            _findSubjectsClient = _busControl.CreateRequestClient<FindSubjectsQuery>();
            _removeSubjectClient = _busControl.CreateRequestClient<RemoveSubjectCommand>();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _busControl.StopAsync();
        }

        [Test]
        public async Task TestSaveSubject()
        {
            var person = _personFiller.FillPerson();
            var saveCommand = new SaveSubjectCommand {Subject = person};
            var saveResponse =
                await _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(saveCommand);
            var personResult = saveResponse.Message;
            if (personResult.Error != null) Assert.Fail(personResult.Error);
            Assert.NotNull(personResult.Subject);
            Assert.AreNotSame(personResult.Subject, person);
            Assert.AreEqual(person.Id, personResult.Subject.Id);
            var company = _companyFiller.FillCompany();
            saveCommand = new SaveSubjectCommand {Subject = company};
            saveResponse =
                await _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(saveCommand);
            if (saveResponse.Message.Error != null) Assert.Fail(saveResponse.Message.Error);
            var subject = saveResponse.Message.Subject;
            Assert.NotNull(subject);
            Assert.AreNotSame(company, subject);
            Assert.AreEqual(company, subject);
        }

        [Test]
        public async Task TestFindSubject()
        {
            // create a test person and test company
            var person = _personFiller.FillPerson();
            var company = _companyFiller.FillCompany();
            var savePersonCommand = new SaveSubjectCommand {Subject = person};
            var saveCompanyCommand = new SaveSubjectCommand {Subject = company};
            var savePersonTask = _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(savePersonCommand);
            var saveCompanyTask = _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(saveCompanyCommand);

            // since we can, we'll test parallel commands and queries
            await Task.WhenAll(savePersonTask, saveCompanyTask);

            // verify test data creation
            var savePersonMessage = savePersonTask.Result.Message;
            var saveCompanyMessage = saveCompanyTask.Result.Message;
            if (savePersonMessage.Error != null) Assert.Fail(savePersonMessage.Error);
            if (!savePersonMessage.Success) Assert.Fail("Unable to save test person data");
            person = (Person) savePersonMessage.Subject;
            if (saveCompanyMessage.Error != null) Assert.Fail(saveCompanyMessage.Error);
            if (!saveCompanyMessage.Success) Assert.Fail("Unable to save test company data");
            company = (Company) saveCompanyMessage.Subject;

            // verify that we can retrieve both a person and a company, and that retrieving all subjects returns both people and companies
            var findAllQuery = new FindSubjectsQuery();
            var findPersonQuery = new FindSubjectsQuery {SubjectId = person.Id};
            var findCompanyQuery = new FindSubjectsQuery {SubjectId = company.Id};
            var findAllTask = _findSubjectsClient.GetResponse<FindSubjectsQueryResult>(findAllQuery);
            var findPersonTask = _findSubjectsClient.GetResponse<FindSubjectsQueryResult>(findPersonQuery);
            var findCompanyTask = _findSubjectsClient.GetResponse<FindSubjectsQueryResult>(findCompanyQuery);

            // parallel again
            await Task.WhenAll(findAllTask, findPersonTask, findCompanyTask);

            var findAllMessage = findAllTask.Result.Message;
            var findPersonMessage = findPersonTask.Result.Message;
            var findCompanyMessage = findCompanyTask.Result.Message;
            ;

            // check for errors
            if (findAllMessage.Error != null)
                Assert.Fail(findAllMessage.Error);
            if (findPersonMessage.Error != null)
                Assert.Fail(findPersonMessage.Error);
            if (findCompanyMessage.Error != null)
                Assert.Fail(findCompanyMessage.Error);

            // verify that find subjects returns both people and companies
            if (findAllMessage.Error != null)
                Assert.Fail(findAllMessage.Error);
            Assert.IsTrue(findAllMessage.Results.Contains(person));
            Assert.IsTrue(findAllMessage.Results.Contains(company));
                
            // verify that if a subject is a person, a Person object is returned
            if (findPersonMessage.Error != null)
                Assert.Fail(findPersonMessage.Error);
            Assert.IsTrue(findPersonMessage.Results.Count == 1);
            // AreEqual() will also ensure that the result is of the correct type, since a generic Subject can't be equal to a Person
            Assert.AreEqual(person, findPersonMessage.Results.Single());

            // verify that if a subject is a company, a Company object is returned
            if (findCompanyMessage.Error != null)
                Assert.Fail(findCompanyMessage.Error);
            Assert.IsTrue(findCompanyMessage.Results.Count == 1);
            Assert.AreEqual(company, findCompanyMessage.Results.Single());
        }

        [Test]
        public async Task TestRemoveSubject()
        {
            var person = _personFiller.FillPerson();
            var company = _companyFiller.FillCompany();
            var savePersonCommand = new SaveSubjectCommand {Subject = person};
            var saveCompanyCommand = new SaveSubjectCommand {Subject = company};
            var savePersonTask = _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(savePersonCommand);
            var saveCompanyTask = _saveSubjectClient.GetResponse<SaveSubjectCommandResult>(saveCompanyCommand);
            await Task.WhenAll(savePersonTask, saveCompanyTask);
            var savePersonMessage = savePersonTask.Result.Message;
            var saveCompanyMessage = saveCompanyTask.Result.Message;
            person = (Person) savePersonMessage.Subject;
            company = (Company) saveCompanyMessage.Subject;
            Assert.NotNull(person);
            Assert.NotNull(company);
            var removePersonCommand = new RemoveSubjectCommand {SubjectId = person.Id};
            var removeCompanyCommand = new RemoveSubjectCommand {SubjectId = company.Id};
            var removePersonTask = _removeSubjectClient.GetResponse<RemoveSubjectCommandResult>(removePersonCommand);
            var removeCompanyTask = _removeSubjectClient.GetResponse<RemoveSubjectCommandResult>(removeCompanyCommand);
            await Task.WhenAll(removePersonTask, removeCompanyTask);
            var removePersonMessage = removePersonTask.Result.Message;
            var removeCompanyMessage = removeCompanyTask.Result.Message;
            if (removePersonMessage.Error != null) Assert.Fail(removePersonMessage.Error);
            if (removeCompanyMessage.Error != null) Assert.Fail(removeCompanyMessage.Error);
            Assert.IsTrue(removePersonMessage.Success);
            Assert.IsTrue(removeCompanyMessage.Success);
        }
    }
}
