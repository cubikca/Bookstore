using System;

namespace Bookstore.Domains.Book.Commands
{
    public class RemovePublisherCommand
    {
        public Guid PublisherId { get; set; } 
    }
}