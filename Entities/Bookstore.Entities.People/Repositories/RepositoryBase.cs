using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.People.Repositories
{
    public abstract class RepositoryBase<TModel, TEntity> : IRepository<TModel> 
        where TModel : class, IDomainObject, new() 
        where TEntity : class, IEntity, new()
    {
        protected readonly IDbContextFactory<PeopleContext> DbFactory;
        protected readonly IMapper Mapper;
        protected readonly ILogger<RepositoryBase<TModel, TEntity>> Logger;

        protected RepositoryBase(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<RepositoryBase<TModel, TEntity>> logger)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
            Logger = logger;
        }


        public virtual async Task<TModel> Save(TModel model)
        {
            try
            {
                using var scope =
                    new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == model.Id);
                if (entity == null)
                {
                    entity = Activator.CreateInstance<TEntity>();
                    entity.Id = model.Id;
                    await db.Set<TEntity>().AddAsync(entity);
                }
                Mapper.Map(model, entity);
                entity.Created = DateTime.Now;
                entity.Updated = DateTime.Now;
                if (entity.CreatedBy == null) entity.CreatedBy = Thread.CurrentPrincipal?.Identity?.Name ?? "Anonymous";
                entity.UpdatedBy = Thread.CurrentPrincipal?.Identity?.Name ?? "Anonymous";
                await db.SaveChangesAsync();
                var result = await Find(entity.Id);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to save Entity of type {typeof(TEntity).Name}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public virtual async Task<TModel> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id && !e.Deleted);
                return entity != null ? Mapper.Map<TModel>(entity) : null;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to retrieve data for Entity of type {typeof(TEntity).Name}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public virtual async Task<ICollection<TModel>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                return Mapper.Map<List<TModel>>(await db.Set<TEntity>().Where(e => !e.Deleted).ToListAsync());
            }
            catch (Exception ex)
            {
                var msg = $"Unable to retrieve all data for Entity of type {typeof(TEntity).Name}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public virtual async Task<bool> Remove(Guid id)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id);
                if (entity == null) return false;
                entity.Deleted = true;
                entity.Updated = DateTime.Now;
                entity.UpdatedBy = Thread.CurrentPrincipal?.Identity?.Name ?? "Anonymous";
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to remove Entity of type {typeof(TEntity).Name}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }
    }
}
