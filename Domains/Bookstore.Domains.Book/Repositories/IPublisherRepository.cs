using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IPublisherRepository
    {
        Publisher SavePublisher(Publisher publisher);
        IList<Publisher> FindAllPublishers();
        Publisher FindPublisherById(Guid publisherId);
        bool RemovePublisher(Guid publisherId);
    }
}
