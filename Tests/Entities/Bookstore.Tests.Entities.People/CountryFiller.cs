using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Entities.People
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
