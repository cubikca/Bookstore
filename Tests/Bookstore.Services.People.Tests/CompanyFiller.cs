using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Services.People.Tests
{
    public class CompanyFiller : FillerBase
    {
        public Company FillCompany()
        {
            var filler = new Filler<Company>();
            filler.Setup(CompanySetup);
            return filler.Create();
        }
    }
}
