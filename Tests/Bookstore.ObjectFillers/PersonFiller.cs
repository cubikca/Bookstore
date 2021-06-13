using System;
using System.Diagnostics.Contracts;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class PersonFiller
    {
        private readonly Filler<Person> _personFiller;

        public PersonFiller()
        {
            _personFiller = new Filler<Person>();
            _personFiller
                .Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.FamilyName).Use(new MnemonicString(1, 4, 10))
                .OnProperty(p => p.GivenNames).Use(new Collectionizer<string, MnemonicString>(1, 3))
                .OnProperty(p => p.KnownAs).Use(new Collectionizer<string, MnemonicString>(0, 1))
                .OnProperty(p => p.MailingAddress).Use<RandomAddress>()
                .OnProperty(p => p.StreetAddress).Use<RandomAddress>()
                .OnProperty(p => p.EmailAddress).Use<RandomEmail>()
                .OnProperty(p => p.PhoneNumber).Use<RandomPhone>()
                .OnProperty(p => p.OnlinePresence).Use<RandomOnlinePresence>()
                .OnProperty(p => p.Title).Use(new MnemonicString(1, 2, 3))
                .OnProperty(p => p.Suffix).Use(new MnemonicString(1, 2, 3));
        }

        public Person FillPerson()
        {
            return _personFiller.Create();
        }
    }
}