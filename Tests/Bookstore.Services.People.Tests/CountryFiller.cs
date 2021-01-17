using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public class CountryFiller : FillerBase
    {
        public Country FillCountry()
        {
            var filler = new Filler<Country>();
            filler.Setup(CountrySetup);
            return filler.Create();
        }
    }
}
