using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class CountryFiller
    {
        private readonly Filler<Country> _countryFiller;

        public CountryFiller()
        {
            _countryFiller = new Filler<Country>();
            _countryFiller
                .Setup(true)
                .OnProperty(c => c.Id).Use(Guid.NewGuid)
                .OnProperty(c => c.Abbreviation).Use(new MnemonicString(1, 2, 3))
                .OnProperty(c => c.Name).Use(new MnemonicString(1, 4, 10));
        }

        public Country FillCountry()
        {
            return _countryFiller.Create();
        }
    }
}