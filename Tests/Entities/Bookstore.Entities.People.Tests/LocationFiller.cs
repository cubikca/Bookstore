using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.People.Tests
{
    public class LocationFiller : FillerBase
    {
        private readonly Filler<Location> _filler;

        public LocationFiller()
        {
            _filler = new Filler<Location>();
            _filler.Setup(LocationSetup);
        }

        public Location FillLocation()
        {
            return _filler.Create();
        }
    }
}
