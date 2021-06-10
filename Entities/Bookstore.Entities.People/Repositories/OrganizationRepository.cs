using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.People.Repositories
{
    public class OrganizationRepository : RepositoryBase<Organization, Models.Organization>, IOrganizationRepository
    {
        private readonly ILocationRepository _locations;
        
        public OrganizationRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILocationRepository locations, ILogger<RepositoryBase<Organization, Models.Organization>> logger) : base(dbFactory, mapper, logger)
        {
            _locations = locations;
        }
        
        public override async Task<Organization> Save(Organization model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var company = await base.Save(model);
                var entity = await db.Organizations.SingleAsync(c => c.Id == company.Id);
                model.Locations ??= new List<Location>();
                foreach (var location in model.Locations)
                {
                    var saved = await _locations.Save(location);
                    var locationEntity = await db.Locations.SingleAsync(l => l.Id == saved.Id);
                    locationEntity.Organization = entity;
                    locationEntity.OrganizationId = entity.Id;
                    await db.SaveChangesAsync();
                }
                foreach (var location in entity.Locations.ToList())
                {
                    if (model.Locations.All(l => l.Id != location.Id))
                    {
                        location.Organization = null;
                        location.OrganizationId = null;
                        await db.SaveChangesAsync();
                        await _locations.Remove(location.Id);
                    }
                }
                var result = await Find(entity.Id);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = "Unable to save Entity of type Company";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<bool> Remove(Guid id)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Organizations.SingleAsync(c => c.Id == id && !c.Deleted);
                if (entity == null) return false;
                foreach (var location in entity.Locations)
                {
                    location.Organization = null;
                    location.OrganizationId = null;
                    await _locations.Remove(location.Id);
                }
                entity.Deleted = true;
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                var msg = "Unable to remove Entity of type Organization";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }    
        }
    }
}