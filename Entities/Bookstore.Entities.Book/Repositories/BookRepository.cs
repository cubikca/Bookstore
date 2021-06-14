using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.Book;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.Book.Queries;
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
                var entity = await db.Books.SingleOrDefaultAsync(b => b.Id == model.Id && !b.Deleted);
                if (entity == null)
                {
                    entity = new Models.Book {Id = model.Id};
                    await db.Books.AddAsync(entity);
                }
                Mapper.Map(model, entity);
                if (model.Publisher != null)
                {
                    var publisher = await _publishers.Save(model.Publisher);
                    entity.Publisher = await db.Publishers.FindAsync(publisher.Id);
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
                    var authorEntity = await db.Authors.FindAsync(authorModel.Id);
                    if (entity.Authors == null || entity.Authors.All(a => a.Id != authorModel.Id))
                    {
                        entity.Authors ??= new List<Models.Author>();
                        entity.Authors.Add(authorEntity);
                    }
                }
                foreach (var author in entity.Authors.ToList())
                {
                    if (model.Authors.All(a => a.Id != author.Id))
                        entity.Authors.Remove(author);
                }
                await db.SaveChangesAsync();
                var result = Mapper.Map<Domains.Book.Models.Book>(entity);
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

        public override async Task<Domains.Book.Models.Book> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Books.SingleOrDefaultAsync(b => b.Id == id && !b.Deleted);
                var book = Mapper.Map<Domains.Book.Models.Book>(entity);
                return book;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public override async Task<ICollection<Domains.Book.Models.Book>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.Books.Where(b => !b.Deleted).ToListAsync();
                var books = Mapper.Map<List<Domains.Book.Models.Book>>(entities);
                return books;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            } 
        }
    }
}
