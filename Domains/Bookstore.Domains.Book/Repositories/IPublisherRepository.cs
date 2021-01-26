using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IPublisherRepository
    {
        Task<Publisher> SavePublisher(Publisher publisher);
        Task<IList<Publisher>> FindAllPublishers();
        Task<Publisher> FindPublisherById(Guid publisherId);
        Task<bool> RemovePublisher(Guid publisherId);
    }
}
