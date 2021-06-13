using System;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class PublisherFiller
    {
        private FillerSetup _publisherSetup;
        
        public PublisherFiller()
        {
            var filler = new Filler<Publisher>();
            _publisherSetup = filler.Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.Profile).Use(() => new OrganizationFiller().FillOrganization())
                .Result;
        }

        public Publisher FillPublisher()
        {
            var filler = new Filler<Publisher>();
            filler.Setup(_publisherSetup);
            return filler.Create();
        }
    }
}