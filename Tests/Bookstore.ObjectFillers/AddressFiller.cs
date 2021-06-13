using System;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class AddressFiller
    {
        private readonly Filler<Address> _addressFiller;

        public AddressFiller()
        {
            _addressFiller = new Filler<Address>();
            _addressFiller
                .Setup(true)
                .OnProperty(a => a.Id).Use(Guid.NewGuid)
                .OnProperty(a => a.City).Use<CityName>()
                .OnProperty(a => a.Street1).Use<StreetName>()
                .OnProperty(a => a.Street2).Use<StreetName>()
                .OnProperty(a => a.PostalCode).Use(new MnemonicString(1, 6, 6))
                .OnProperty(a => a.Province).Use(() => new ProvinceFiller().FillProvince())
                .OnProperty(a => a.Country).Use(() => new CountryFiller().FillCountry());
        }

        public Address FillAddress()
        {
            return _addressFiller.Create();
        }
    }
}