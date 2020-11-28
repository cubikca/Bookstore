using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.People.Repositories
{
    public class CountryRepository : RepositoryBase, ICountryRepository
    {
        private readonly ILogger<CountryRepository> _logger;

        public CountryRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<CountryRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
        }

        public async Task<Country> SaveCountry(Country country)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                if (country.Id == default)
                    country.Id = Guid.NewGuid();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Id == country.Id);
                var add = false;
                if (entity == null)
                {
                    entity = new Models.Country();
                    add = true;
                }
                Mapper.Map(country, entity);
                if (add) await db.Countries.AddAsync(entity);
                await db.SaveChangesAsync();
                return Mapper.Map<Country>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save country data");
                throw new PeopleException("Unable to save country data", ex);
            }
        }

        public async Task<Country> FindCountryById(Guid countryId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Id == countryId);
                if (entity == null) return null;
                return Mapper.Map<Country>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve country data");
                throw new PeopleException("Unable to retrieve country data");
            }
        }

        public async Task<IList<Country>> FindAllCountries()
        {
            try
            {
                using var db = DbFactory.CreateDbContext();
                var entities = await db.Countries.ToListAsync();
                return Mapper.Map<List<Country>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve country data");
                throw new PeopleException("Unable to retrieve country data");
            }
        }

        public async Task<bool> RemoveCountry(Guid countryId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.SingleOrDefaultAsync(c => c.Id == countryId);
                if (entity == null) return false;
                if (entity.Provinces != null)
                {
                    foreach (var province in entity.Provinces.ToList())
                        db.Provinces.Remove(province);
                }
                db.Countries.Remove(entity);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove country data");
                throw new PeopleException("Unable to remove country data", ex);
            }
        }

        public async Task<Province> SaveProvince(Province province)
        {
            try
            {
                using var db = DbFactory.CreateDbContext();
                if (province.Id == default)
                    province.Id = Guid.NewGuid();
                var entity = await  db.Provinces.SingleOrDefaultAsync(p => p.Id == province.Id);
                var countryEntity = await db.Countries.SingleOrDefaultAsync(c => c.Id == province.Country.Id);
                var add = false;
                if (entity == null)
                {
                    entity = new Models.Province();
                    add = true;
                }
                Mapper.Map(province, entity);
                entity.Country = countryEntity ?? throw new PeopleException("Invalid country specified while saving province data");
                if (add) await db.Provinces.AddAsync(entity);
                await db.SaveChangesAsync();
                return await FindProvinceById(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save province data");
                throw new PeopleException("Unable to save province data", ex);
            }
        }

        public async Task<Province> FindProvinceById(Guid provinceId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces.SingleOrDefaultAsync(p => p.Id == provinceId);
                if (entity == null) return null;
                return Mapper.Map<Province>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve province data");
                throw new PeopleException("Unable to save province data", ex);
            }
        }

        public async Task<IList<Province>> FindProvincesByCountryId(Guid countryId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.Provinces.Where(p => p.Country.Id == countryId).ToListAsync();
                return Mapper.Map<List<Province>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve province data");
                throw new PeopleException("Unable to retrieve province data", ex);
            }
        }

        public async Task<bool> RemoveProvince(Guid provinceId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Provinces.SingleOrDefaultAsync(p => p.Id == provinceId);
                if (entity == null) return false;
                db.Provinces.Remove(entity);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove province data");
                throw new PeopleException("Unable to remove province data", ex);
            }
        }
    }
}
