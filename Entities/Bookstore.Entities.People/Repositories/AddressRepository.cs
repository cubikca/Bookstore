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
    public class AddressRepository : RepositoryBase<Address, Models.Address>, IAddressRepository
    {
        static IQueryable<Models.Address> AddressQuery(PeopleContext db) =>
            db.Addresses
                .Include(a => a.Country)
                .Include(a => a.Province).ThenInclude(p => p.Country)
                .AsQueryable();
        
        private readonly ICountryRepository _countries;
        private readonly IProvinceRepository _provinces;
        
        public AddressRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ICountryRepository countries, IProvinceRepository provinces, ILogger<RepositoryBase<Address, Models.Address>> logger) : base(dbFactory, mapper, logger)
        {
            _provinces = provinces;
            _countries = countries;
        }

        public override async Task<Address> Save(Address model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var address = await base.Save(model);
                var entity = await AddressQuery(db).SingleOrDefaultAsync(a => a.Id == address.Id && !a.Deleted);
                if (entity == null || entity.Deleted) return null;
                if (model.Country != null)
                {
                    var country = await _countries.Save(model.Country);
                    entity.Country = await db.Countries.FindAsync(country.Id);
                    entity.CountryId = country.Id;
                }
                else
                    entity.Deleted = true;
                if (model.Province != null)
                {
                    var province = await _provinces.Save(model.Province);
                    entity.Province = await db.Provinces.FindAsync(province.Id);
                    entity.ProvinceId = province.Id;
                }
                else
                {
                    entity.Province = null;
                    entity.ProvinceId = null;
                }
                if (model.Country != null)
                {
                    var country = await _countries.Save(model.Country);
                    entity.Country = await db.Countries.FindAsync(country.Id);
                    entity.CountryId = country.Id;
                }
                await db.SaveChangesAsync();
                var result = Mapper.Map<Address>(await AddressQuery(db).SingleAsync(a => a.Id == model.Id));
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to save Entity of type {nameof(Address)}";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }    
        }

        public override async Task<Address> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await AddressQuery(db).SingleOrDefaultAsync(a => a.Id == id && !a.Deleted);
                return Mapper.Map<Address>(entity);
            }
            catch (Exception ex)
            {
                var msg = "Unable to retrieve Entity data of type Address";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<ICollection<Address>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await AddressQuery(db).Where(a => !a.Deleted).ToListAsync();
                return Mapper.Map<List<Address>>(entities);
            }
            catch (Exception ex)
            {
                var msg = "Failed to retrieve Entity data of type Address";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<bool> Remove(Guid id)
        {
            try
            {
                // Address is referred to in a few places, so we need to set them null manually
                using var scope =
                    new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                foreach (var person in db.People.Where(p => p.StreetAddressId == id))
                {
                    person.StreetAddress = null;
                    person.StreetAddressId = null;
                }
                foreach (var location in db.Locations.Where(l => l.StreetAddressId == id))
                {
                    location.StreetAddress = null;
                    location.StreetAddressId = null;
                }
                foreach (var person in db.People.Where(p => p.MailingAddressId == id))
                {
                    person.MailingAddress = null;
                    person.MailingAddressId = null;
                }
                foreach (var location in db.Locations.Where(l => l.MailingAddressId == id))
                {
                    location.MailingAddress = null;
                    location.MailingAddressId = null;
                }
                await db.SaveChangesAsync();
                var result = await base.Remove(id);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = "Failed to remove Entity of type Address";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }
    }
}