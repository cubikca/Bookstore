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
    public class PublisherRepository : IPublisherRepository
    {
        private readonly BookContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<PublisherRepository> _log;
        
        public PublisherRepository(IDbContextFactory<BookContext> dbFactory, IMapper mapper, ILogger<PublisherRepository> log)
        {
            _db = dbFactory.CreateDbContext();
            _mapper = mapper;
            _log = log;
        }
        
        public async Task<Publisher> SavePublisher(Publisher publisher)
        {
            try
            {
                Models.Publisher entity = null;
                if (publisher.Id == default) publisher.Id = Guid.NewGuid();
                entity = await _db.Publishers.SingleOrDefaultAsync(p => p.Id == publisher.Id);
                if (entity == null)
                {
                    entity = new Models.Publisher {Id = publisher.Id};
                    await _db.Publishers.AddAsync(entity);
                }
                _mapper.Map(publisher, entity);
                await _db.SaveChangesAsync();
                return await FindPublisherById(entity.Id);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while saving publisher: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<IList<Publisher>> FindAllPublishers()
        {
            try
            {
                var entities = await _db.Publishers.ToListAsync();
                return _mapper.Map<List<Publisher>>(entities);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while saving publisher: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<Publisher> FindPublisherById(Guid publisherId)
        {
            try
            {
                var entity = await _db.Publishers.SingleOrDefaultAsync(p => p.Id == publisherId);
                return entity != null
                    ? _mapper.Map<Publisher>(entity)
                    : null;
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while saving publisher: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public async Task<bool> RemovePublisher(Guid publisherId)
        {
            try
            {
                var entity = await _db.Publishers.SingleOrDefaultAsync(p => p.Id == publisherId);
                if (entity == null) return false;
                _db.Publishers.Remove(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;
                _log.LogError(ex, "Error while saving publisher: {Message}", message);
                throw new EntityException(ex.Message, ex);
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}