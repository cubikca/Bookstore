using System;
using System.Linq;
using System.Text;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class RandomAddress : IRandomizerPlugin<Address>
    {
        private readonly AddressFiller _filler;
        
        public RandomAddress()
        {
            _filler = new AddressFiller();
        }
        
        public Address GetValue()
        {
            return _filler.FillAddress();
        }
    }

    public class RandomEmail : IRandomizerPlugin<EmailAddress>
    {
        const string AlphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
        
        private readonly Filler<EmailAddress> _filler;

        public RandomEmail()
        {
            _filler = new Filler<EmailAddress>();
        }

        private char RandomAlphaNumberic =>
            AlphaNumeric.ToCharArray().ElementAt(new Random().Next(0, AlphaNumeric.Length));

        public EmailAddress GetValue()
        {
            var domainString = new StringBuilder();
            var domainRoot = new StringBuilder();
            var email = new StringBuilder();
            var len = new Random().Next(8, 16);
            for (var i = 0; i < len; i++)
                domainString.Append(RandomAlphaNumberic);
            len = new Random().Next(3, 4);
            for (var i = 0; i < len; i++)
                domainRoot.Append(RandomAlphaNumberic);
            len = new Random().Next(4, 10);
            for (var i = 0; i < len; i++)
                email.Append(RandomAlphaNumberic);
            return new EmailAddress {Id = Guid.NewGuid(), Email = $"{email}@{domainString}.{domainRoot}"};
        }
    }

    public class RandomPhone : IRandomizerPlugin<PhoneNumber>
    {
        public PhoneNumber GetValue()
        {
            return new PhoneNumber
            {
                Id = Guid.NewGuid(), AreaCode = $"{new Random().Next(100, 1000):000}",
                Phone = $"{new Random().Next(100, 1000):000}-{new Random().Next(0, 10000):0000}"
            };
        }
    }

    public class RandomOnlinePresence : IRandomizerPlugin<OnlinePresence>
    {
        private readonly Filler<OnlinePresence> _filler;

        public RandomOnlinePresence()
        {
            _filler = new Filler<OnlinePresence>();
            _filler.Setup(true)
                .OnProperty(o => o.Id).Use(Guid.NewGuid)
                .OnProperty(o => o.Facebook).Use<MnemonicString>()
                .OnProperty(o => o.Twitter).Use<MnemonicString>()
                .OnProperty(o => o.Instagram).Use<MnemonicString>()
                .OnProperty(o => o.LinkedIn).Use<MnemonicString>();
        }

        public OnlinePresence GetValue()
        {
            return _filler.Create();
        }
    }

    public class RandomPerson : IRandomizerPlugin<Person>
    {
        private readonly PersonFiller _personFiller;

        public RandomPerson()
        {
            _personFiller = new PersonFiller();
        }

        public Person GetValue()
        {
            return _personFiller.FillPerson();
        }
    }

    public class RandomLocation : IRandomizerPlugin<Location>
    {
        private readonly LocationFiller _locationFiller;

        public RandomLocation()
        {
            _locationFiller = new LocationFiller();
        }

        public Location GetValue()
        {
            return _locationFiller.FillLocation();
        }
    }
}