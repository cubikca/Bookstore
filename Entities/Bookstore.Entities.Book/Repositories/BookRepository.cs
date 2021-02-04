using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using Bookstore.Domains.Book.Repositories;
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.Book.Repositories
{
    public class BookRepository : IBookRepository
    {
        private BookContext _db;
        private ILogger<BookRepository> _log;
        private IMapper _mapper;

        public BookRepository(IDbContextFactory<BookContext> dbFactory, ILogger<BookRepository> logger, IMapper mapper)
        {
            _db = dbFactory.CreateDbContext();
            _log = logger;
            _mapper = mapper;
        }

        public async Task<Domains.Book.Models.Book> SaveBook(Domains.Book.Models.Book book)
        {
            try
            {
                if (book.Id == Guid.Empty)
                    book.Id = Guid.NewGuid();
                Models.Book entity = null;
                Models.Publisher publisherEntity = null;
                if (book.Publisher != null)
                {
                    if (book.Publisher.Id == Guid.Empty)
                        book.Publisher.Id = Guid.NewGuid();
                    publisherEntity = await _db.Publishers.SingleOrDefaultAsync(p => p.Id == book.Publisher.Id);
                    if (publisherEntity == null)
                    {
                        publisherEntity = new Models.Publisher {Id = book.Publisher.Id};
                        await _db.Publishers.AddAsync(publisherEntity);
                    }
                    _mapper.Map(book.Publisher, publisherEntity);
                }
                entity = await _db.Books.SingleOrDefaultAsync(b => b.Id == book.Id);
                if (entity == null)
                {
                    entity = new Models.Book() {Id = book.Id};
                    await _db.Books.AddAsync(entity);
                }
                _mapper.Map(book, entity);
                if (book.Authors?.Any() == true)
                {
                    foreach (var author in book.Authors)
                    {
                        Models.Author authorEntity = null;
                        if (author.Id == Guid.Empty)
                            author.Id = Guid.NewGuid();
                        authorEntity = await _db.Authors.SingleOrDefaultAsync(a => a.Id == author.Id);
                        if (authorEntity == null)
                        {
                            authorEntity = new Models.Author {Id = author.Id};
                            entity.Authors ??= new List<Models.Author>();
                            await _db.Authors.AddAsync(authorEntity);
                            entity.Authors.Add(authorEntity);
                        }
                        _mapper.Map(author, authorEntity);
                    }
                }                
                if (book.Publisher == null && entity.Publisher != null)
                {
                    _db.Publishers.Remove(entity.Publisher);
                    entity.Publisher = null;
                }
                if (publisherEntity != null && entity.Publisher == null)
                    entity.Publisher = publisherEntity;
                foreach (var authorEntity in entity.Authors.ToList()
                    .Where(authorEntity => book.Authors?.All(b => b.Id != authorEntity.Id) != false))
                {
                    // no authors, or all authors don't match
                    _db.Authors.Remove(authorEntity);
                    entity.Authors.Remove(authorEntity);
                }
                await _db.SaveChangesAsync();
                return await FindBookById(entity.Id);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError("Error while saving book: {Message}", message);
                throw new EntityException(message, ex);
            }
        }

        public async Task<IList<Domains.Book.Models.Book>> FindAllBooks()
        {
            try
            {
                var books = await _db.Books.ToListAsync();
                return _mapper.Map<List<Domains.Book.Models.Book>>(books);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError("Error while retrieving books: {Message}", message);
                throw new EntityException(message, ex);
            }
        }

        public async Task<Domains.Book.Models.Book> FindBookById(Guid bookId)
        {
            try
            {
                var book = await _db.Books.SingleOrDefaultAsync(b => b.Id == bookId);
                return book != null ? _mapper.Map<Domains.Book.Models.Book>(book) : null;
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError("Error while retrieving books: {Message}", message);
                throw new EntityException(message, ex);
            }
        }

        public async Task<bool> RemoveBook(Guid bookId)
        {
            try
            {
                var entity = await _db.Books.SingleOrDefaultAsync(b => b.Id == bookId);
                if (entity == null)
                    return false;
                _db.Books.Remove(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError("Error while retrieving books: {Message}", message);
                throw new EntityException(message, ex);
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
