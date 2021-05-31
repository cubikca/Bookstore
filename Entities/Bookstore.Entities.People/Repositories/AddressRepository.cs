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
    // there is a circular dependency between a CountryRepository and a ProvinceRepository
    // this repository is the most logical second choice if not possible to use repositories for Country and Province
    public class AddressRepository : RepositoryBase, IAddressRepository, ICountryRepository, IProvinceRepository
    {
        private readonly ILogger<AddressRepository> _logger;
        
        public AddressRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<AddressRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
        }

        public async Task<Address> SaveAddress(Address address)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Addresses.SingleOrDefaultAsync(a => a.Id == address.Id);
                if (entity == null)
                {
                    entity = new Models.Address {Id = address.Id};
                    await db.Addresses.AddAsync(entity);
                }
                Mapper.Map(address, entity);
                var countryEntity =
                    address.Country != null 
                        ? await db.Countries.SingleOrDefaultAsync(c => c.Abbreviation == address.Country.Abbreviation) 
                        : null;
                if (address.Country != null && countryEntity != null && countryEntity.Name != address.Country.Name)
                    throw new PeopleException("Not updating country as part of Address update");
                var savedCountry = await SaveCountry(address.Country);
                entity.Country = await db.Countries.SingleAsync(c => c.Abbreviation == savedCountry.Abbreviation);
                var provinceEntity =
                    address.Province != null
                        ? await db.Provinces.SingleOrDefaultAsync(p =>
                            p.Abbreviation == address.Province.Abbreviation &&
                            p.Country.Abbreviation == address.Province.Country.Abbreviation)
                        : null;
                if (address.Province != null
                    && provinceEntity != null
                    && address.Province.Country.Abbreviation == provinceEntity.Country.Abbreviation
                    && address.Province.Abbreviation == provinceEntity.Abbreviation
                    && address.Province.Name != provinceEntity.Name)
                    throw new PeopleException("Not updating Province as part of Address update");
                var savedProvince = await SaveProvince(address.Province);
                entity.Province = await db.Provinces.SingleAsync(p =>
                    p.Country.Abbreviation == savedProvince.Country.Abbreviation &&
                    p.Abbreviation == savedProvince.Abbreviation);
                await db.SaveChangesAsync();
                var result = await FindAddressById(entity.Id); 
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save Address");
                throw new PeopleException("Unable to save Address", ex);
            }
        }

        public async Task<ICollection<Address>> FindAllAddresses()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                return Mapper.Map<List<Address>>(db.Addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Address data");
                throw new PeopleException("Unable to retrieve Address data");
            }
        }

        public async Task<Address> FindAddressById(Guid addressId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Addresses.SingleAsync(a => a.Id == addressId);
                return Mapper.Map<Address>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Address data");
                throw new PeopleException("Unable to retrieve Address data", ex);
            }
        }

        public async Task<bool> RemoveAddress(Guid addressId)
        {
            /*
             * Removing an address should also set referring fields to null: Person.StreetAddress, Person.MailingAddress,
             * Location.StreetAddress, Location.MailingAddress. While this could be accomplished using CASCADE SET NULL
             * in an RDBMS that supports multiple cascade paths, I think that it is the wrong approach. First, it is
             * not clear from code that this is happening. Second, reliance on a vendor-specific feature defeats the
             * purpose of using a platform-agnostic library like EF Core.
             */
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Addresses.SingleOrDefaultAsync(a => a.Id == addressId);
                if (entity == null) return false;
                var locations = await db.Locations
                    .Where(l => l.MailingAddressId == addressId || l.StreetAddressId == addressId).ToListAsync();
                locations.ForEach(l =>
                {
                    if (l.MailingAddressId == addressId) l.MailingAddressId = null;
                    if (l.StreetAddressId == addressId) l.StreetAddressId = null;
                });
                var people = await db.People
                    .Where(p => p.MailingAddress.Id == addressId || p.StreetAddress.Id == addressId).ToListAsync();
                people.ForEach(p =>
                {
                    if (p.MailingAddress?.Id == addressId) p.MailingAddress = null;
                    if (p.StreetAddress?.Id == addressId) p.StreetAddress = null;
                });
                db.Addresses.Remove(entity);
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove Address");
                throw new PeopleException("Unable to remove Address", ex);
            }
        }

        public async Task<Country> SaveCountry(Country country)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Abbreviation == country.Abbreviation);
                if (entity == null)
                {
                    entity = new Models.Country {Abbreviation = country.Abbreviation};
                    await db.Countries.AddAsync(entity);
                }
                Mapper.Map(country, entity);
                await db.SaveChangesAsync();
                var result = await FindCountryByAbbreviation(entity.Abbreviation);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save Country");
                throw new PeopleException("Unable to save Country", ex);
            }
        }

        public async Task<Country> FindCountryByAbbreviation(string abbreviation)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Abbreviation == abbreviation);
                return Mapper.Map<Country>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Country");
                throw new PeopleException("Unable to retrieve Country", ex);
            }
        }

        public async Task<ICollection<Country>> FindAllCountries()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                return Mapper.Map<List<Country>>(db.Countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Countries");
                throw new PeopleException("Unable to retrieve Countries", ex);
            }
        }

        public async Task<bool> RemoveCountry(string abbreviation)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Abbreviation == abbreviation);
                if (entity == null) return false;
                // like addresses, provinces must be disconnected from the country first and then deleted as part of
                // the country deletion
                var addresses = await db.Addresses.Where(a => a.Country.Abbreviation == abbreviation).ToListAsync();
                foreach (var address in addresses)
                    address.Country = null;
                await db.SaveChangesAsync();
                var provinces = await db.Provinces.Where(p => p.Country.Abbreviation == abbreviation).ToListAsync();
                foreach (var province in provinces)
                    province.Country = null;
                await db.SaveChangesAsync();
                db.Countries.Remove(entity);
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove Country");
                throw new PeopleException("Unable to remove Country", ex);
            }
        }

        public async Task<Province> SaveProvince(Province province)
        {
            try
            {
                if (province.Country == null) throw new ArgumentException(nameof(province.Country));
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces.SingleOrDefaultAsync(p => p.Abbreviation == province.Abbreviation);
                if (entity == null)
                {
                    var countryEntity =
                        await db.Countries.SingleOrDefaultAsync(c => c.Abbreviation == province.Country.Abbreviation);
                    if (countryEntity == null)
                    {
                        var savedCountry = await SaveCountry(province.Country);
                        countryEntity = await db.Countries.SingleAsync(c => c.Abbreviation == savedCountry.Abbreviation);
                    }
                    entity = new Models.Province(province.Abbreviation, countryEntity.Abbreviation);
                    await db.Provinces.AddAsync(entity);
                }
                Mapper.Map(province, entity);
                await db.SaveChangesAsync();
                var result = await FindProvinceByAbbreviation(entity.Abbreviation);
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save Province");
                throw new PeopleException("Unable to save Province", ex);
            }
        }

        public async Task<Province> FindProvinceByAbbreviation(string abbreviation)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces.SingleOrDefaultAsync(p => p.Abbreviation == abbreviation);
                return Mapper.Map<Province>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Province");
                throw new PeopleException("Unable to retrieve Province", ex);
            }
        }

        public async Task<ICollection<Province>> FindProvincesByCountryAbbreviation(string countryAbbreviation)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var provinces = await db.Provinces.Where(p => p.Country.Abbreviation == countryAbbreviation).ToListAsync();
                return Mapper.Map<List<Province>>(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Provinces for Country");
                throw new PeopleException("Unable to retrieve Provinces for Country", ex);
            }
        }

        public async Task<ICollection<Province>> FindAllProvinces()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                return Mapper.Map<List<Province>>(await db.Provinces.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve Provinces");
                throw new PeopleException("Unable to retrieve Provinces", ex);
            }
        }

        public async Task<bool> RemoveProvince(string abbreviation)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces.SingleOrDefaultAsync(p => p.Abbreviation == abbreviation);
                if (entity == null) return false;
                // clear references to this Province
                var addresses = await db.Addresses.Where(p => p.Province.Abbreviation == abbreviation).ToListAsync();
                foreach (var address in addresses)
                    address.Province = null;
                db.Provinces.Remove(entity);
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove Province");
                throw new PeopleException("Unable to remove Province");
            }
        }
    }
}