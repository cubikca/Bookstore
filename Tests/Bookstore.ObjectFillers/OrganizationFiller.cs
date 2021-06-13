using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class OrganizationFiller 
    {
        private Filler<Organization> _filler;

        public OrganizationFiller()
        {
            _filler = new Filler<Organization>();
            _filler.Setup(true)
                .OnProperty(c => c.Id).Use(Guid.NewGuid)
                .OnProperty(c => c.OrganizationName).Use<MnemonicString>()
                .OnProperty(c => c.Locations).Use(new Collectionizer<Location, RandomLocation>(1, 3));
        }
        
        public Organization FillOrganization()
        {
            return _filler.Create();
        }
    }
}