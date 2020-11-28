using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Entities.Book
{
    public class EntityException : Exception
    {
        public EntityException(string message, Exception ex) : base(message, ex) {}
    }
}
