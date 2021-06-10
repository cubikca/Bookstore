using System;
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
                var entity = await db.Addresses.SingleOrDefaultAsync(a => a.Id == address.Id);
                if (entity == null)
                    return null;
                if (model.Country != null)
                {
                    var country = await _countries.Save(model.Country);
                    entity.Country = await db.Countries.SingleOrDefaultAsync(c => c.Id == country.Id);
                    entity.CountryId = country?.Id;
                }
                else
                    entity.Deleted = true;
                if (model.Province != null)
                {
                    var province = await _provinces.Save(model.Province);
                    entity.Province = await db.Provinces.SingleOrDefaultAsync(p => p.Id == province.Id);
                    entity.ProvinceId = province?.Id;
                }
                else
                {
                    entity.Province = null;
                    entity.ProvinceId = null;
                }
                if (model.Country != null)
                {
                    var country = await _countries.Save(model.Country);
                    entity.Country = await db.Countries.SingleOrDefaultAsync(c => c.Id == country.Id);
                    entity.CountryId = country.Id;
                }
                await db.SaveChangesAsync();
                var result = await Find(entity.Id);
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

        public override async Task<bool> Remove(Guid id)
        {
            using var scope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await using var db = DbFactory.CreateDbContext();
            try
            {
                // Address is referred to in a few places, so we need to set them null manually
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
                var result = await base.Remove(id);
                await db.SaveChangesAsync();
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