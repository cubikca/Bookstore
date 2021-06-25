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

        public async Task<Publisher> FindPublisherForBook(Guid bookId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var book = await db.Books.SingleOrDefaultAsync(b => b.Id == bookId);
                return book == null ? null : Mapper.Map<Publisher>(book.Publisher);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}