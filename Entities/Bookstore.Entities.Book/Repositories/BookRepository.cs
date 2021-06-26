using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.Book;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.Book.Repositories
{
    public class BookRepository : RepositoryBase<Domains.Book.Models.Book, Models.Book>, IBookRepository
    {
        private readonly IPublisherRepository _publishers;
        private readonly IAuthorRepository _authors;
        
        public BookRepository(IDbContextFactory<BookContext> dbFactory, IMapper mapper, IPublisherRepository publishers, IAuthorRepository authors, ILogger<BookRepository> logger) 
            : base(dbFactory, mapper, logger)
        {
            _publishers = publishers;
            _authors = authors;
        }

        public override async Task<Domains.Book.Models.Book> Save(Domains.Book.Models.Book model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var book = await base.Save(model);
                var entity = await db.Books.SingleAsync(b => b.Id == book.Id);
                if (model.Publisher != null)
                {
                    var publisher = await _publishers.Save(model.Publisher);
                    entity.Publisher = await db.Publishers.SingleOrDefaultAsync(p => p.Id == publisher.Id && !p.Deleted);
                    entity.PublisherId = publisher.Id;
                }
                else
                {
                    entity.Publisher = null;
                    entity.PublisherId = null;
                }
                await db.SaveChangesAsync();
                model.Authors ??= new List<Author>();
                foreach (var author in model.Authors)
                {
                    var authorModel = await _authors.Save(author);
                    var authorEntity = await db.Authors.SingleOrDefaultAsync(a => a.Id == authorModel.Id && !a.Deleted);
                    if (entity.Authors.All(a => a.Id != authorModel.Id))
                        entity.Authors.Add(authorEntity);
                }
                foreach (var author in entity.Authors.ToList())
                {
                    if (model.Authors.All(a => a.Id != author.Id))
                        entity.Authors.Remove(author);
                }
                await db.SaveChangesAsync();
                var result = await Find(entity.Id);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = "Unable to save Entity of type Book";
                Logger.LogError(ex, msg);
                throw new BookException(msg, ex);
            }
        }
    }
}
