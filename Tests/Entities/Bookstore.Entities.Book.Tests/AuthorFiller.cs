using System;
using System.Linq;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.Entities.Book.Tests
{
    public class AuthorFiller
    {
        private FillerSetup _authorSetup;

        public AuthorFiller()
        {
            _authorSetup = new Filler<Author>()
                .Setup(true)
                .OnProperty(a => a.Id).Use(Guid.NewGuid)
                .OnProperty(a => a.Details.Id).Use(Guid.NewGuid)
                .OnProperty(a => a.Salary).Use(() => new Random().Next(100000, 150000))
                .OnProperty(a => a.Books).IgnoreIt()
                .Result;
        }

        public Author FillAuthor()
        {
            var filler = new Filler<Author>();
            filler.Setup(_authorSetup);
            return filler.Create();
        }
    }
}