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
    /***
     * This is a default implementation of a repository sufficient for simple objects with simple properties.
     * It works really well for such objects, and shouldn't be modified for specialized cases farther down the
     * hierarchy.
     *
     * In particular, this repository doesn't know anything about the object it is processing, and so can't automatically
     * include navigation properties. There are two solutions: the lazy solution and the better solution. The lazy solution
     * is to add lazy loading proxies to all of your database contexts (a la EF6). The better solution is to specify
     * which fields to load prior to automapping - by overriding the appropriate methods in the concrete repository.
     */
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
                var entity = await db.Set<TEntity>().FindAsync(model.Id);
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
                var result = Mapper.Map<TModel>(await db.Set<TEntity>().FindAsync(entity.Id));
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
                var entity = await db.Set<TEntity>().FindAsync(id);
                if (entity == null) return null;
                var result = entity.Deleted
                    ? null
                    : Mapper.Map<TModel>(entity);
                return result;
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
                var results =  Mapper.Map<List<TModel>>(await db.Set<TEntity>().Where(x => !x.Deleted).ToListAsync());
                return results;
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
                var entity = await db.Set<TEntity>().FindAsync(id);
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
