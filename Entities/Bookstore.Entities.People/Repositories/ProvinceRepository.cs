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
    public class ProvinceRepository : RepositoryBase<Province, Models.Province>, IProvinceRepository
    {
        private readonly ICountryRepository _countries;
        
        public ProvinceRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ICountryRepository countries, ILogger<RepositoryBase<Province, Models.Province>> logger) : base(dbFactory, mapper, logger)
        {
            _countries = countries;
        }

        public override async Task<Province> Save(Province model)
        {
            try
            {
                using var scope =
                    new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var province = await base.Save(model);
                var country = await _countries.Save(model.Country);
                var entity = await db.Provinces.FindAsync(province.Id);
                entity.Country = await db.Countries.FindAsync(country.Id);
                entity.CountryId = country.Id;
                await db.SaveChangesAsync();
                var result = Mapper.Map<Province>(await db.Provinces
                    .Include(p => p.Country)
                    .SingleAsync(p => p.Id == model.Id));
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = "Unable to save Entity of type Province";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<Province> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces
                    .Include(p => p.Country)
                    .SingleOrDefaultAsync(p => p.Id == id && !p.Deleted);
                return Mapper.Map<Province>(entity);
            }
            catch (Exception ex)
            {
                var msg = "Unable to retrieve Entity data of type Province";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<ICollection<Province>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.Provinces
                    .Where(p => !p.Deleted)
                    .ToListAsync();
                return Mapper.Map<List<Province>>(entities);
            }
            catch (Exception ex)
            {
                var msg = "Failed to retrieve Entity data of type Province";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public async Task<ICollection<Province>> FindByCountry(Guid countryId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var provinces = await db.Provinces
                    .Where(p => p.CountryId == countryId)
                    .ToListAsync();
                var result = Mapper.Map<List<Province>>(provinces);
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to retrieve Provinces for Country {countryId}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }
    }
}