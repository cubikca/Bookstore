using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class ProvinceFiller
    {
        private readonly Filler<Province> _provinceFiller;

        public ProvinceFiller()
        {
            _provinceFiller = new Filler<Province>();
            _provinceFiller
                .Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.Abbreviation).Use(new MnemonicString(1, 2, 3))
                .OnProperty(p => p.Country).Use(() => new CountryFiller().FillCountry())
                .OnProperty(p => p.Name).Use(new MnemonicString(1, 4, 10));
        }

        public Province FillProvince()
        {
            return _provinceFiller.Create();
        }
    }
}