using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Services.People
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
