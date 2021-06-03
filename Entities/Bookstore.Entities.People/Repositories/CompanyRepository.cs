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
    public class CompanyRepository : RepositoryBase<Company, Models.Company>, ICompanyRepository
    {
        private readonly ILocationRepository _locations;
        private readonly IAddressRepository _addresses;
        
        public CompanyRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILocationRepository locations, IAddressRepository addresses, ILogger<RepositoryBase<Company, Models.Company>> logger) : base(dbFactory, mapper, logger)
        {
            _locations = locations;
            _addresses = addresses;
        }
        
        public override async Task<Company> Save(Company model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var company = await base.Save(model);
                var entity = await db.Companies.SingleAsync(c => c.Id == company.Id);
                model.Locations ??= new List<Location>();
                foreach (var location in model.Locations)
                {
                    var saved = await _locations.Save(location);
                    var locationEntity = await db.Locations.SingleAsync(l => l.Id == saved.Id);
                    locationEntity.Company = entity;
                    locationEntity.CompanyId = entity.Id;
                    await db.SaveChangesAsync();
                }
                foreach (var location in entity.Locations.ToList())
                {
                    if (model.Locations.All(l => l.Id != location.Id))
                    {
                        location.Company = null;
                        location.CompanyId = null;
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
                var entity = await db.Companies.SingleAsync(c => c.Id == id && !c.Deleted);
                if (entity == null) return false;
                foreach (var location in entity.Locations)
                {
                    location.Company = null;
                    location.CompanyId = null;
                    await _locations.Remove(location.Id);
                }
                entity.Deleted = true;
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                var msg = "Unable to remove Entity of type Company";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }    
        }
    }
}