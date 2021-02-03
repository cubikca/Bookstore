using System;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.Book.Tests
{
    public class BookFiller
    {
        private FillerSetup _bookSetup;

        public BookFiller()
        {
            _bookSetup = new Filler<Domains.Book.Models.Book>()
                .Setup(true)
                .OnProperty(b => b.Id).IgnoreIt()
                .OnProperty(b => b.Authors).IgnoreIt()
                .OnProperty(b => b.Publisher).Use(new Publisher { Id = Guid.NewGuid() })
                .Result;
        }

        public Domains.Book.Models.Book FillBook()
        {
            var filler = new Filler<Domains.Book.Models.Book>();
            filler.Setup(_bookSetup);
            return filler.Create();
        }
    }
}