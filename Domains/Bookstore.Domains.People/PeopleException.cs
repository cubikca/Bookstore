using System;

namespace Bookstore.Domains.People
{
    public class PeopleException : Exception
    {
        public PeopleException()  { }

        public PeopleException(string message) : base(message) { }
        public PeopleException(string message, Exception exception) : base(message, exception) { }
    }
}
