using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class CompanyFiller 
    {
        private Filler<Company> _filler;

        public CompanyFiller()
        {
            _filler = new Filler<Company>();
            _filler.Setup(true)
                .OnProperty(c => c.Id).Use(Guid.NewGuid)
                .OnProperty(c => c.CompanyName).Use<MnemonicString>()
                .OnProperty(c => c.Locations).Use(new Collectionizer<Location, RandomLocation>(1, 3));
        }
        
        public Company FillCompany()
        {
            return _filler.Create();
        }
    }
}