using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.Book.Repositories
{
    public class PublisherRepository : RepositoryBase<Publisher, Models.Publisher>, IPublisherRepository
    {
        public PublisherRepository(IDbContextFactory<BookContext> dbFactory, IMapper mapper, ILogger<PublisherRepository> logger)
            : base(dbFactory, mapper, logger)
        {
        }
    }
}