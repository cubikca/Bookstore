using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class LocationFiller
    {
        private readonly Filler<Location> _locationFiller;

        public LocationFiller()
        {
            _locationFiller = new Filler<Location>();
            _locationFiller.Setup(true)
                .OnProperty(l => l.Id).Use(Guid.NewGuid)
                .OnProperty(l => l.CompanyId).Use((Guid?) null)
                .OnProperty(l => l.StreetAddress).Use<RandomAddress>()
                .OnProperty(l => l.MailingAddress).Use<RandomAddress>()
                .OnProperty(l => l.Contacts).Use(new Collectionizer<Person, RandomPerson>(1, 3));
        }

        public Location FillLocation()
        {
            return _locationFiller.Create();
        }
    }
}