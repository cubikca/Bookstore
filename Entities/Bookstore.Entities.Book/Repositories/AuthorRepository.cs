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
    public class AuthorRepository : IAuthorRepository
    {
        private readonly BookContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorRepository> _log;
        
        public AuthorRepository(BookContext db, IMapper mapper, ILogger<AuthorRepository> log)
        {
            _db = db;
            _mapper = mapper;
            _log = log;
        }
        
        public async Task<Author> SaveAuthor(Author author)
        {
            try
            {
                Models.Author entity = null;
                if (author.Id != default)
                    entity = await _db.Authors.SingleOrDefaultAsync(a => a.Id == author.Id);
                entity ??= new Models.Author();
                _mapper.Map(author, entity);
                await _db.SaveChangesAsync();
                return _mapper.Map<Author>(entity);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while saving author: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<IList<Author>> FindAllAuthors()
        {
            try
            {
                var entities = await _db.Authors.ToListAsync();
                return _mapper.Map<List<Author>>(entities);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while retrieving authors: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<Author> FindAuthorById(Guid authorId)
        {
            try
            {
                var entity = await _db.Authors.SingleOrDefaultAsync(a => a.Id == authorId);
                return entity != null
                    ? _mapper.Map<Author>(entity)
                    : null;
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while retrieving authors: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<bool> RemoveAuthor(Guid authorId)
        {
            try
            {
                var entity = await _db.Authors.SingleOrDefaultAsync(a => a.Id == authorId);
                if (entity == null) return false;
                _db.Authors.Remove(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                 var message = ex.GetBaseException().Message;
                 _log.LogError(ex, "Error while retrieving authors: {Message}", message);
                 throw new EntityException(ex.Message, ex);
            }
        }
    }
}