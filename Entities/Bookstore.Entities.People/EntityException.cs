using System;

namespace Bookstore.Entities.People
{
    public class EntityException : Exception
    {
        public EntityException(string message, Exception ex) : base(message, ex) {}
    }
}
