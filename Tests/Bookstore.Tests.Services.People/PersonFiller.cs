using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Services.People
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
