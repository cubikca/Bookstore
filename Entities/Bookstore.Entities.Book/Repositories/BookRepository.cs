using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
                if (book.Publisher != null)
                {
                    Models.Publisher publisherEntity = null;
                    if (book.Publisher.Id != Guid.Empty)
                        publisherEntity = await _db.Publishers.SingleOrDefaultAsync(p => p.Id == book.Publisher.Id);
                    if (publisherEntity == null)
                    {
                        publisherEntity = new Models.Publisher {Id = Guid.NewGuid()};
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
                if (book.Publisher == null && entity.Publisher != null)
                {
                    _db.Publishers.Remove(entity.Publisher);
                    entity.Publisher = null;
                }
                await _db.SaveChangesAsync();
                return _mapper.Map<Domains.Book.Models.Book>(entity);
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
