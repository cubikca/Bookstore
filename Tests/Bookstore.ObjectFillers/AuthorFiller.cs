using System;
using Bookstore.Domains.Book.Models;
using Tynamix.ObjectFiller;

namespace Bookstore.ObjectFillers
{
    public class AuthorFiller
    {
        private FillerSetup _authorSetup;

        public AuthorFiller()
        {
            _authorSetup = new Filler<Author>()
                .Setup(true)
                .OnProperty(a => a.Id).Use(Guid.NewGuid)
                .OnProperty(a => a.Profile).Use(() => new PersonFiller().FillPerson())
                .OnProperty(a => a.Salary).Use(() => new Random().Next(100000, 150000))
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