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
        private ILogger _log;
        private IMapper _mapper;
        private IPersonRepository _people;
        private ICompanyRepository _companies;

        public BookRepository(BookContext db, ILogger logger, IMapper mapper)
        {
            _db = db;
            _log = logger;
            _mapper = mapper;
        }

        public async Task<Domains.Book.Models.Book> SaveBook(Domains.Book.Models.Book book)
        {
            try
            {
                Models.Book entity = null; 
                if (book.Id != Guid.Empty)
                    entity = await _db.Books.SingleOrDefaultAsync(b => b.Id == book.Id);
                entity ??= new Models.Book();
                _mapper.Map(book, entity);
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
                var book = await _db.Books.ToListAsync();
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
    }
}
