using System;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.Book.Tests
{
    public class PublisherFiller
    {
        private FillerSetup _publisherSetup;
        
        public PublisherFiller()
        {
            var filler = new Filler<Publisher>();
            _publisherSetup = filler.Setup(true)
                .OnProperty(p => p.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.Details.Id).Use(Guid.NewGuid)
                .OnProperty(p => p.Books).IgnoreIt()
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