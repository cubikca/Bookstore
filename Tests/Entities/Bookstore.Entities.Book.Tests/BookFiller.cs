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
                .OnProperty(b => b.Id).Use(Guid.NewGuid)
                .OnProperty(b => b.Authors).Use(new Collectionizer<Author, RandomAuthor>(1, 3))
                .OnProperty(b => b.Publisher).Use<RandomPublisher>()
                .Result;
        }

        public Domains.Book.Models.Book FillBook()
        {
            var filler = new Filler<Domains.Book.Models.Book>();
            filler.Setup(_bookSetup);
            return filler.Create();
        }
    }

    public class RandomAuthor : IRandomizerPlugin<Author>
    {
        private AuthorFiller _authorFiller = new();
        
        public Author GetValue()
        {
            return _authorFiller.FillAuthor();
        }
    }

    public class RandomPublisher : IRandomizerPlugin<Publisher>
    {
        private readonly PublisherFiller _publisherFiller = new();
        
        public Publisher GetValue()
        {
            return _publisherFiller.FillPublisher();
        }
    }
}
