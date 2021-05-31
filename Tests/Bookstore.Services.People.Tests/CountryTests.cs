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
using Bookstore.Domains.People.Repositories;
using GreenPipes;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public class CountryTests
    {
        private IBusControl _busControl;
        private Filler<Country> _countryFiller;
        private Filler<Province> _provinceFiller;
        private IRequestClient<SaveCountryCommand> _saveCountryClient;
        private IRequestClient<FindCountriesQuery> _findCountriesClient;
        private IRequestClient<RemoveCountryCommand> _removeCountryClient;
        private IRequestClient<SaveProvinceCommand> _saveProvinceClient;
        private IRequestClient<FindProvincesQuery> _findProvincesClient;
        private IRequestClient<RemoveProvinceCommand> _removeProvinceClient;
        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _countryFiller = new Filler<Country>();
            _provinceFiller = new Filler<Province>();
            _busControl = Bus.Factory.CreateUsingRabbitMq(rmq =>
            {
                rmq.Host(new Uri("amqp://localhost:5672/people"), host =>
                {
                    host.Username("brian");
                    host.Password("development");
                });
                rmq.UseBsonSerializer();
            });
            _saveCountryClient = _busControl.CreateRequestClient<SaveCountryCommand>();
            _findCountriesClient = _busControl.CreateRequestClient<FindCountriesQuery>();
            _removeCountryClient = _busControl.CreateRequestClient<RemoveCountryCommand>();
            _saveProvinceClient = _busControl.CreateRequestClient<SaveProvinceCommand>();
            _findProvincesClient = _busControl.CreateRequestClient<FindProvincesQuery>();
            _removeProvinceClient = _busControl.CreateRequestClient<RemoveProvinceCommand>();       
            await _busControl.StartAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _busControl.StopAsync();
        }

        [Test]
        public async Task TestSave()
        {
            try
            {
                var country = _countryFiller.Create();
                var saveCountryCommand = new SaveCountryCommand {Country = country};
                var saveCountryResponse =
                    await _saveCountryClient.GetResponse<SaveCountryCommandResult>(saveCountryCommand);
                var saveCountryResult = saveCountryResponse.Message;
                Assert.AreNotSame(country, saveCountryResult.Country);
                Assert.AreEqual(country, saveCountryResult.Country);
                country = saveCountryResult.Country;
                var province = _provinceFiller.Create();
                province.Country = country;
                var saveProvinceCommand = new SaveProvinceCommand {Province = province};
                var saveProvinceResponse =
                    await _saveProvinceClient.GetResponse<SaveProvinceCommandResult>(saveProvinceCommand);
                var saveProvinceResult = saveProvinceResponse.Message;
                Assert.AreNotSame(province, saveProvinceResult.Province);
                Assert.AreEqual(province, saveProvinceResult.Province);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task TestFind()
        {
            try
            {
                var country = _countryFiller.Create();
                var saveCountryCommand = new SaveCountryCommand {Country = country};
                var saveCountryResponse =
                    await _saveCountryClient.GetResponse<SaveCountryCommandResult>(saveCountryCommand);
                country = saveCountryResponse.Message.Country;
                var province = _provinceFiller.Create();
                province.Country = country;
                var saveProvinceCommand = new SaveProvinceCommand {Province = province};
                var saveProvinceResponse =
                    await _saveProvinceClient.GetResponse<SaveProvinceCommandResult>(saveProvinceCommand);
                var province1 = saveProvinceResponse.Message.Province;
                province = _provinceFiller.Create();
                province.Country = country;
                saveProvinceCommand = new SaveProvinceCommand {Province = province};
                saveProvinceResponse = 
                    await _saveProvinceClient.GetResponse<SaveProvinceCommandResult>(saveProvinceCommand);
                var province2 = saveProvinceResponse.Message.Province;
                var findCountryQuery = new FindCountriesQuery {CountryAbbreviation = country.Abbreviation};
                var findCountryResponse =
                    await _findCountriesClient.GetResponse<FindCountriesQueryResult>(findCountryQuery);
                var findCountryResult = findCountryResponse.Message;
                Country foundCountry = null;
                Assert.AreEqual(1, findCountryResult.Results.Count);
                foundCountry = findCountryResult.Results.Single();
                Assert.AreNotSame(country, foundCountry);
                Assert.AreEqual(country, foundCountry);
                var findProvinceQuery = new FindProvincesQuery {ProvinceAbbreviation = province1?.Abbreviation};
                var findProvinceResponse =
                    await _findProvincesClient.GetResponse<FindProvincesQueryResult>(findProvinceQuery);
                var findProvinceResult = findProvinceResponse.Message;
                Assert.AreEqual(1, findProvinceResult.Results.Count);
                var foundProvince = findProvinceResult.Results.Single();
                Assert.NotNull(foundProvince);
                Assert.AreNotSame(province1, foundProvince);
                Assert.AreEqual(province1, foundProvince);
                var findProvincesQuery = new FindProvincesQuery {CountryAbbreviation = country.Abbreviation};
                var findProvincesResponse =
                    await _findProvincesClient.GetResponse<FindProvincesQueryResult>(findProvincesQuery);
                var findProvincesResult = findProvincesResponse.Message;
                var foundProvinces = findProvincesResult.Results;
                Assert.NotNull(foundProvinces);
                Assert.IsTrue(foundProvinces.Contains(province1));
                Assert.IsTrue(foundProvinces.Contains(province2));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task TestRemove()
        {
            var saveCountryClient = _busControl.CreateRequestClient<SaveCountryCommand>();
            var saveProvinceClient = _busControl.CreateRequestClient<SaveProvinceCommand>();
            var removeCountryClient = _busControl.CreateRequestClient<RemoveCountryCommand>();
            var removeProvinceClient = _busControl.CreateRequestClient<RemoveProvinceCommand>();
            var findProvinceClient = _busControl.CreateRequestClient<FindProvincesQuery>();
            var findCountryClient = _busControl.CreateRequestClient<FindCountriesQuery>();
            var country = _countryFiller.Create();
            var saveCountryCommand = new SaveCountryCommand {Country = country};
            var saveCountryResponse =
                await saveCountryClient.GetResponse<SaveCountryCommandResult>(saveCountryCommand);
            country = saveCountryResponse.Message.Country;
            var province1 = _provinceFiller.Create();
            var province2 = _provinceFiller.Create();
            province1.Country = country;
            province2.Country = country;
            var saveProvince1Command = new SaveProvinceCommand {Province = province1};
            var saveProvince2Command = new SaveProvinceCommand {Province = province2};
            var saveProvince1Task = saveProvinceClient.GetResponse<SaveProvinceCommandResult>(saveProvince1Command);
            var saveProvince2Task = saveProvinceClient.GetResponse<SaveProvinceCommandResult>(saveProvince2Command);
            await Task.WhenAll(saveProvince1Task, saveProvince2Task);
            var saveProvince1Response = saveProvince1Task.Result;
            var saveProvince2Response = saveProvince2Task.Result;
            province1 = saveProvince1Response.Message.Province;
            province2 = saveProvince2Response.Message.Province;

            // remove province 2 by itself
            var removeProvince2Command = new RemoveProvinceCommand {ProvinceAbbreviation = province2.Abbreviation};
            var removeProvince2Response =
                await removeProvinceClient.GetResponse<RemoveProvinceCommandResult>(removeProvince2Command);
            Assert.IsTrue(removeProvince2Response.Message.Success);
            var findProvince2Query = new FindProvincesQuery {ProvinceAbbreviation = province2.Abbreviation};
            var findProvince2Response =
                await findProvinceClient.GetResponse<FindProvincesQueryResult>(findProvince2Query);
            Assert.AreEqual(0, findProvince2Response.Message.Results.Count);

            // province 1 should be deleted when country is deleted
            var removeCountryCommand = new RemoveCountryCommand {CountryAbbreviation = country.Abbreviation};
            await removeCountryClient.GetResponse<RemoveCountryCommandResult>(removeCountryCommand);
            var findCountryQuery = new FindCountriesQuery {CountryAbbreviation = country.Abbreviation};
            var findProvince1Query = new FindProvincesQuery {ProvinceAbbreviation = province1.Abbreviation};
            var findCountryTask = findCountryClient.GetResponse<FindCountriesQueryResult>(findCountryQuery);
            var findProvince1Task = findProvinceClient.GetResponse<FindProvincesQueryResult>(findProvince1Query);
            await Task.WhenAll(findCountryTask, findProvince1Task);
            var findCountryResponse = findCountryTask.Result;
            var findProvince1Response = findProvince1Task.Result;
            Assert.AreEqual(0, findCountryResponse.Message.Results.Count);
            Assert.AreEqual(0, findProvince1Response.Message.Results.Count);
        }
    }
}
