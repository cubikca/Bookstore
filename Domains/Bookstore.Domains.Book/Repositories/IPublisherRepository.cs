using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Book.Repositories
{
    public interface IPublisherRepository  : IRepository<Publisher>
    {
    }
}
