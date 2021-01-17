using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.People.Tests
{
    public class PersonFiller : FillerBase
    {
        public Person FillPerson()
        {
            var filler = new Filler<Person>();
            filler.Setup(PersonSetup);
            return filler.Create();
        }
    }
}
