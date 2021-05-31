using System;
using System.Linq;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public abstract class FillerBase
    {
        protected readonly FillerSetup PhoneSetup;
        protected readonly FillerSetup EmailSetup;
        protected readonly FillerSetup ProvinceSetup;
        protected readonly FillerSetup OnlineSetup;
        protected readonly FillerSetup CountrySetup;
        protected readonly FillerSetup AddressSetup;
        protected readonly FillerSetup LocationSetup;
        protected readonly FillerSetup PersonSetup;
        protected readonly FillerSetup CompanySetup;

        public FillerBase()
        {
            Filler<OnlinePresence> oFiller = new Filler<OnlinePresence>();
            OnlineSetup = oFiller.Setup(true)
                .OnProperty(o => o.Id).Use(Guid.NewGuid)
                .OnType<string>().Use(new MnemonicString(1, 5, 10))
                .Result;
            Filler<PhoneNumber> phFiller = new Filler<PhoneNumber>();
            PhoneSetup = phFiller.Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.AreaCode).Use($"{new Random().Next(1, 999):000}")
                .OnProperty(p => p.Phone).Use($"{new Random().Next(1, 9999999):000-0000}")
                .OnProperty(p => p.Extension).IgnoreIt()
                .Result;
            Filler<EmailAddress> emFiller = new Filler<EmailAddress>();
            EmailSetup = emFiller.Setup(true)
                .OnProperty(e => e.Id).Use(Guid.NewGuid)
                .OnProperty(e => e.Email).UseDefault()
                .OnProperty(e => e.Primary).Use(false)
                .OnProperty(e => e.Verified).Use(false)
                .Result;
            Filler<Province> prFiller = new Filler<Province>();
            ProvinceSetup = prFiller.Setup(true)
                .OnProperty(p => p.Abbreviation).Use(new MnemonicString(1, 2, 3))
                .OnProperty(p => p.Name).Use(new MnemonicString(1, 5, 10))
                .Result;
            Filler<Country> cyFiller = new Filler<Country>();
            CountrySetup = cyFiller.Setup(true)
                .OnProperty(c => c.Abbreviation).Use(new MnemonicString(1, 2, 3))
                .OnProperty(c => c.Name).Use(new MnemonicString(1, 5, 10))
                .Result;
            Filler<Address> aFiller = new Filler<Address>();
            AddressSetup = aFiller.Setup(true)
                .OnProperty(a => a.Id).Use(Guid.NewGuid)
                .OnProperty(a => a.Street1).Use<StreetName>()
                .OnProperty(a => a.Street2).Use<StreetName>()
                .OnProperty(a => a.City).Use<CityName>()
                .OnProperty(a => a.Province).Use(ProvinceSetup)
                .Result;
            Filler<Person> pFiller = new Filler<Person>();
            PersonSetup = pFiller.Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.FamilyName).Use(new MnemonicString(1, 5, 10))
                .OnProperty(p => p.Name).IgnoreIt()
                .OnProperty(p => p.GivenNames).Use(Randomizer<string>.Create(new RealNames(NameStyle.FirstName), 1).ToList())
                .OnProperty(p => p.KnownAs).Use(Randomizer<string>.Create(new RealNames(NameStyle.FirstName), 1).ToList())
                .OnProperty(p => p.EmailAddress).Use(EmailSetup)
                .OnProperty(p => p.Initial).Use(new MnemonicString(1, 1, 1))
                .OnProperty(p => p.Suffix).Use(new MnemonicString(1, 2, 3))
                .OnProperty(p => p.Title).Use(new MnemonicString(1, 2, 3))
                .OnProperty(p => p.PhoneNumber).Use(PhoneSetup)
                .OnProperty(p => p.OnlinePresence).Use(OnlineSetup)
                .Result;
            Filler<Location> lFiller = new Filler<Location>();
            LocationSetup = lFiller.Setup()
                .OnProperty(l => l.Id).Use(Guid.NewGuid)
                .OnProperty(l => l.StreetAddress).Use(AddressSetup)
                .OnProperty(l => l.MailingAddress).Use(AddressSetup)
                .OnProperty(l => l.Contacts).Use(new Collectionizer<Person, PersonRandomizer>(new PersonRandomizer(PersonSetup), 1, 2))
                .Result;
            Filler<Company> cFiller = new Filler<Company>();
            CompanySetup = cFiller.Setup()
                .OnProperty(c => c.Id).Use(Guid.NewGuid)
                .OnProperty(c => c.CompanyName).Use(new MnemonicString(2, 5, 10))
                .OnProperty(c => c.Locations).Use(new Collectionizer<Location, LocationRandomizer>(new LocationRandomizer(LocationSetup), 1, 2))
                .OnProperty(c => c.StreetAddress).IgnoreIt()
                .OnProperty(c => c.MailingAddress).IgnoreIt()
                .OnProperty(c => c.Name).IgnoreIt()
                .OnProperty(c => c.FullName).IgnoreIt()
                .OnProperty(c => c.Contact).IgnoreIt()
                .Result;
        }
   }

    public class LocationRandomizer : IRandomizerPlugin<Location>
    {
        private readonly Filler<Location> _filler;

        public LocationRandomizer()
        {
        }

        public LocationRandomizer(FillerSetup locationSetup)
        {
            _filler = new Filler<Location>();
            _filler.Setup(locationSetup);
        }

        public Location GetValue()
        {
            return _filler.Create();
        }
    }

    public class PersonRandomizer : IRandomizerPlugin<Person>
    {
        private readonly Filler<Person> _filler;

        public PersonRandomizer()
        {
        }

        public PersonRandomizer(FillerSetup personSetup)
        {
            _filler = new Filler<Person>();
            _filler.Setup(personSetup);
        }

        public Person GetValue()
        {
            return _filler.Create();
        }
    }
}
