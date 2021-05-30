using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                await using var db = DbFactory.CreateDbContext();
                await db.Database.BeginTransactionAsync();
                var entity = await db.Addresses.SingleOrDefaultAsync(a => a.Id == address.Id);
                if (entity == null)
                {
                    entity = new Models.Address {Id = address.Id};
                    await db.Addresses.AddAsync(entity);
                }
                Mapper.Map(address, entity);
                await db.SaveChangesAsync();
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
                            p.CountryId == address.Province.Country.Id)
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
                await db.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save Address");
                throw new PeopleException("Unable to save Address", ex);
            }
        }

        public async Task<ICollection<Address>> FindAllAddresses()
        {
            throw new NotImplementedException();
        }

        public async Task<Address> FindAddressById(Guid addressId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveAddress(Guid addressId)
        {
            throw new NotImplementedException();
        }

        public async Task<Country> SaveCountry(Country country)
        {
            throw new NotImplementedException();
        }

        public async Task<Country> FindCountryById(Guid countryId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Country>> FindAllCountries()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveCountry(Guid countryId)
        {
            throw new NotImplementedException();
        }

        public async Task<Province> SaveProvince(Province province)
        {
            throw new NotImplementedException();
        }

        public async Task<Province> FindProvinceById(Guid provinceId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Province>> FindProvincesByCountryId(Guid countryId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Province>> FindAllProvinces()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveProvince(Guid provinceId)
        {
            throw new NotImplementedException();
        }
    }
}