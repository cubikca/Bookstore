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
            var company = filler.Create();
            // fix up the Location references
            // it won't be possible to test properly if we can't get the client side to look like the server side
            // without taking the server side as gospel
            foreach (var location in company.Locations)
                location.CompanyId = company.Id;
            return company;
        }
    }
}
