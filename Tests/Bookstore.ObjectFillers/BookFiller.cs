using System;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class BookFiller
    {
        private FillerSetup _bookSetup;

        public BookFiller()
        {
            _bookSetup = new Filler<Domains.Book.Models.Book>()
                .Setup(true)
                .OnProperty(b => b.Id).Use(Guid.NewGuid)
                .OnProperty(b => b.ISBN).Use(new MnemonicString(1, 16, 16))
                .OnProperty(b => b.PublishDate).Use(() => new DateTimeRange(new DateTime(1900, 1, 1), new DateTime(2016, 12, 31)).GetValue())
                .OnProperty(b => b.Title).Use(new MnemonicString(3, 4, 10))
                .OnProperty(b => b.Subtitle).Use(new MnemonicString(4, 3, 10))
                .OnProperty(b => b.Edition).UseDefault()
                .OnProperty(b => b.Cost).UseDefault()
                .OnProperty(b => b.Authors).Use(new Collectionizer<Author, RandomAuthor>(1, 3))
                .OnProperty(b => b.Publisher).Use<RandomPublisher>()
                .Result;
        }

        public Domains.Book.Models.Book FillBook()
        {
            var filler = new Filler<Domains.Book.Models.Book>();
            filler.Setup(_bookSetup, true);
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
