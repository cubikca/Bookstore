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
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitWarren;
using RabbitWarren.Messaging;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public class CountryTests
    {
        private RabbitMQConnection _rmqConnection;
        private RabbitMQPublishChannel _publishChannel;
        private RabbitMQConsumer _consumer;
        private Filler<Country> _countryFiller;
        private Filler<Province> _provinceFiller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _countryFiller = new Filler<Country>();
            _provinceFiller = new Filler<Province>();
            var rmqFactory = new RabbitMQConnectionFactory(RabbitMQProtocol.AMQP, "127.0.0.1", "people", 5672, null,
                new ContainerBuilder().Build(), "brian", "development");
            _rmqConnection = rmqFactory.Create();
            _publishChannel = _rmqConnection.OpenPublishChannel("rabbitwarren");
            var consumerChannel =
                _rmqConnection.OpenConsumerChannel("", $"response.{Guid.NewGuid()}", autoDelete: true);
            _consumer = consumerChannel.RegisterDefaultConsumer();
            _consumer.Start();
        }

        [Test]
        public async Task TestSave()
        {
            var country = _countryFiller.Create();
            var saveCountryCommand = new SaveCountryCommand {Country = country};
            var saveCountryResponse =
                await _publishChannel.Request(saveCountryCommand, "people", _consumer.Channel.Queue);
            if (saveCountryResponse is ErrorResult saveCountryError)
                Assert.Fail(saveCountryError.Error);
            if (!(saveCountryResponse is SaveCountryCommandResult saveCountryResult))
                Assert.Fail("Unknown response received to save country command");
            else
            {
                Assert.AreNotSame(country, saveCountryResult.Country);
                Assert.AreEqual(country, saveCountryResult.Country);
                country = saveCountryResult.Country;
            }

            var province = _provinceFiller.Create();
            province.Country = country;
            var saveProvinceCommand = new SaveProvinceCommand {Province = province};
            var saveProvinceResponse =
                await _publishChannel.Request(saveProvinceCommand, "people", _consumer.Channel.Queue);
            if (saveProvinceResponse is ErrorResult saveProvinceError)
                Assert.Fail(saveProvinceError.Error);
            if (!(saveProvinceResponse is SaveProvinceCommandResult saveProvinceResult))
                Assert.Fail("Unknown response received to save province command");
            else
            {
                Assert.AreNotSame(province, saveProvinceResult.Province);
                Assert.AreEqual(province, saveProvinceResult.Province);
            }
        }

        [Test]
        public async Task TestFind()
        {
            var country = _countryFiller.Create();
            var saveCountryCommand = new SaveCountryCommand {Country = country};
            var response = await _publishChannel.Request(saveCountryCommand, "people", _consumer.Channel.Queue);
            if (response is ErrorResult createCountryError)
                Assert.Fail(createCountryError.Error);
            var province = _provinceFiller.Create();
            province.Country = country;
            var saveProvinceCommand = new SaveProvinceCommand {Province = province};
            response = await _publishChannel.Request(saveProvinceCommand, "people", _consumer.Channel.Queue);
            if (response is ErrorResult createProvince1Error)
                Assert.Fail(createProvince1Error.Error);
            Province province1 = null;
            Province province2 = null;
            if (response is SaveProvinceCommandResult createProvince1Result)
                province1 = createProvince1Result.Province;
            province = _provinceFiller.Create();
            province.Country = country;
            saveProvinceCommand = new SaveProvinceCommand {Province = province};
            response = await _publishChannel.Request(saveProvinceCommand, "people", _consumer.Channel.Queue);
            if (response is ErrorResult createProvince2Error)
                Assert.Fail(createProvince2Error.Error);
            if (response is SaveProvinceCommandResult createProvince2Result)
                province2 = createProvince2Result.Province;
            var findCountryQuery = new FindCountriesQuery {CountryId = country.Id};
            Country foundCountry = null;
            response = await _publishChannel.Request(findCountryQuery, "people", _consumer.Channel.Queue);
            if (response is ErrorResult findCountryError)
                Assert.Fail(findCountryError.Error);
            if (response is FindCountriesQueryResult findCountryResult)
            {
                Assert.AreEqual(1, findCountryResult.Results.Count);
                foundCountry = findCountryResult.Results.Single();
            }
            Assert.AreNotSame(country, foundCountry);
            Assert.AreEqual(country, foundCountry);
            var findProvinceQuery = new FindProvincesQuery {ProvinceId = province1?.Id};
            response = await _publishChannel.Request(findProvinceQuery, "people", _consumer.Channel.Queue);
            Province foundProvince = null;
            if (response is ErrorResult findProvinceError)
                Assert.Fail(findProvinceError.Error);
            if (response is FindProvincesQueryResult findProvinceResult)
            {
                Assert.AreEqual(1, findProvinceResult.Results.Count);
                foundProvince = findProvinceResult.Results.Single();
            }
            Assert.NotNull(foundProvince);
            Assert.AreNotSame(province1, foundProvince);
            Assert.AreEqual(province1, foundProvince);
            var findProvincesQuery = new FindProvincesQuery {CountryId = country.Id};
            response = await _publishChannel.Request(findProvincesQuery, "people", _consumer.Channel.Queue);
            IList<Province> foundProvinces = null;
            if (response is FindProvincesQueryResult findProvincesResult)
                foundProvinces = findProvincesResult.Results;
            Assert.NotNull(foundProvinces);
            Assert.IsTrue(foundProvinces.Contains(province1));
            Assert.IsTrue(foundProvinces.Contains(province2));
        }

        [Test]
        public async Task TestRemove()
        {
            var country = _countryFiller.Create();
            var saveCountryCommand = new SaveCountryCommand {Country = country};
            var response = await _publishChannel.Request(saveCountryCommand, "people", _consumer.Channel.Queue);
            if (response is ErrorResult createCountryError)
                Assert.Fail(createCountryError.Error);
            var province1 = _provinceFiller.Create();
            var province2 = _provinceFiller.Create();
            province1.Country = country;
            province2.Country = country;
            var saveProvince1Command = new SaveProvinceCommand {Province = province1};
            var saveProvince2Command = new SaveProvinceCommand {Province = province2};
            var saveProvince1Task = _publishChannel.Request(saveProvince1Command, "people", _consumer.Channel.Queue);
            var saveProvince2Task = _publishChannel.Request(saveProvince2Command, "people", _consumer.Channel.Queue);
            await Task.WhenAll(saveProvince1Task, saveProvince2Task);
            var saveProvince1Response = saveProvince1Task.Result;
            var saveProvince2Response = saveProvince2Task.Result;
            if (saveProvince1Response is ErrorResult saveProvince1Error)
                Assert.Fail(saveProvince1Error.Error);
            if (saveProvince2Response is ErrorResult saveProvince2Error)
                Assert.Fail(saveProvince2Error.Error);
            if (saveProvince1Response is SaveProvinceCommandResult saveProvince1Result)
                province1 = saveProvince1Result.Province;
            if (saveProvince2Response is SaveProvinceCommandResult saveProvince2Result)
                province2 = saveProvince2Result.Province;

            // remove province 2 by itself
            var removeProvince2Command = new RemoveProvinceCommand {ProvinceId = province2.Id};
            var removeProvince2Response = await _publishChannel.Request(removeProvince2Command, "people", _consumer.Channel.Queue);
            if (removeProvince2Response is ErrorResult removeProvince2Error)
                Assert.Fail(removeProvince2Error.Error);
            if (removeProvince2Response is RemoveProvinceCommandResult removeProvince2Result)
                Assert.IsTrue(removeProvince2Result.Success);
            var findProvince2Query = new FindProvincesQuery {ProvinceId = province2.Id};
            var findProvince2Response = await _publishChannel.Request(findProvince2Query, "people", _consumer.Channel.Queue);
            if (findProvince2Response is ErrorResult findProvince2Error)
                Assert.Fail(findProvince2Error.Error);
            if (findProvince2Response is FindProvincesQueryResult findProvince2Result)
                Assert.AreEqual(0, findProvince2Result.Results.Count);

            // province 1 should be deleted when country is deleted
            var removeCountryCommand = new RemoveCountryCommand {CountryId = country.Id};
            var removeCountryResponse = await _publishChannel.Request(removeCountryCommand, "people", _consumer.Channel.Queue);
            if (removeCountryResponse is ErrorResult removeCountryError)
                Assert.Fail(removeCountryError.Error);
            if (removeCountryResponse is RemoveCountryCommandResult removeCountryResult)
                Assert.IsTrue(removeCountryResult.Success);
            var findCountryQuery = new FindCountriesQuery {CountryId = country.Id};
            var findProvince1Query = new FindProvincesQuery {ProvinceId = province1.Id};
            var findCountryTask = _publishChannel.Request(findCountryQuery, "people", _consumer.Channel.Queue);
            var findProvince1Task = _publishChannel.Request(findProvince1Query, "people", _consumer.Channel.Queue);
            await Task.WhenAll(findCountryTask, findProvince1Task);
            var findCountryResponse = findCountryTask.Result;
            var findProvince1Response = findProvince1Task.Result;
            if (findCountryResponse is ErrorResult findCountryError)
                Assert.Fail(findCountryError.Error);
            if (findProvince1Response is ErrorResult findProvince1Error)
                Assert.Fail(findProvince1Error.Error);
            if (findCountryResponse is FindCountriesQueryResult findCountryResult)
                Assert.AreEqual(0, findCountryResult.Results.Count);
            if (findProvince1Response is FindProvincesQueryResult findProvince1Result)
                Assert.AreEqual(0, findProvince1Result.Results.Count);
        }
    }
}
