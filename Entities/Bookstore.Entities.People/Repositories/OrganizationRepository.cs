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
        private static IQueryable<Models.Organization> OrganizationQuery(PeopleContext db) =>
            db.Organizations
                .Include(o => o.Locations).ThenInclude(l => l.MailingAddress).ThenInclude(a => a.Country)
                .Include(o => o.Locations).ThenInclude(l => l.MailingAddress).ThenInclude(a => a.Province).ThenInclude(p => p.Country)
                .Include(o => o.Locations).ThenInclude(l => l.StreetAddress).ThenInclude(a => a.Country)
                .Include(o => o.Locations).ThenInclude(l => l.StreetAddress).ThenInclude(a => a.Province).ThenInclude(p => p.Country)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.GivenNames)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.KnownAs)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.EmailAddress)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.PhoneNumber)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.OnlinePresence)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.MailingAddress).ThenInclude(a => a.Country)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.MailingAddress).ThenInclude(a => a.Province).ThenInclude(p => p.Country)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.StreetAddress).ThenInclude(a => a.Country)
                .Include(o => o.Locations).ThenInclude(l => l.Contacts).ThenInclude(c => c.Contact).ThenInclude(c => c.StreetAddress).ThenInclude(a => a.Province).ThenInclude(p => p.Country)
                .AsQueryable();
        
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
                var organization = await base.Save(model);
                var entity = await OrganizationQuery(db).SingleOrDefaultAsync(o => o.Id == organization.Id);
                model.Locations ??= new List<Location>();
                foreach (var location in model.Locations)
                {
                    var saved = await _locations.Save(location);
                    var locationEntity = await db.Locations.FindAsync(saved.Id);
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
                var result = Mapper.Map<Organization>(await OrganizationQuery(db).SingleAsync(o => o.Id == entity.Id)); 
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
                var entity = await OrganizationQuery(db).SingleOrDefaultAsync(o => o.Id == id && !o.Deleted);
                if (entity == null || entity.Deleted) return false;
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

        public override async Task<Organization> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await OrganizationQuery(db).SingleOrDefaultAsync(o => o.Id == id && !o.Deleted);
                return Mapper.Map<Organization>(entity);
            }
            catch (Exception ex)
            {
                var msg = "Unable to retrieve Entity data of type Organization";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<ICollection<Organization>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await OrganizationQuery(db)
                    .Where(o => !o.Deleted)
                    .ToListAsync();
                return Mapper.Map<List<Organization>>(entities);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}