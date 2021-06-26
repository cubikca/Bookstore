using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.Book.Repositories
{
    public class AuthorRepository : RepositoryBase<Author, Models.Author>, IAuthorRepository
    {
        public AuthorRepository(IDbContextFactory<BookContext> dbFactory, IMapper mapper, ILogger<AuthorRepository> logger) : base(dbFactory, mapper, logger)
        {
        }

        public async Task<ICollection<Author>> FindByBook(Guid bookId)
        {
            try
            {
                var result = new List<Author>();
                await using var db = DbFactory.CreateDbContext();
                var book = await db.Books.SingleOrDefaultAsync(b => b.Id == bookId);
                if (book == null) return result;
                var authors = Mapper.Map<List<Author>>(book.Authors);
                return authors;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}