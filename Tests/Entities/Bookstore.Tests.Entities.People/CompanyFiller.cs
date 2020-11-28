using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Tests.Entities.People
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
