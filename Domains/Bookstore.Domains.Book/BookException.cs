using System;

namespace Bookstore.Domains.Book
{
    public class BookException : Exception
    {
        public BookException()
        {
        }

        public BookException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}