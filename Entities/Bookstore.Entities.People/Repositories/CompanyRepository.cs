using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Address = Bookstore.Entities.People.Models.Address;
using Company = Bookstore.Entities.People.Models.Company;
using Location = Bookstore.Entities.People.Models.Location;

namespace Bookstore.Entities.People.Repositories
{
    public class CompanyRepository : RepositoryBase, ICompanyRepository
    {
        private readonly ILogger<CompanyRepository> _logger;
        private readonly IPersonRepository _people;
        private readonly ILocationRepository _locations;

        public CompanyRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IPersonRepository people, ILocationRepository locations, ILogger<CompanyRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
            _people = people;
            _locations = locations;
        }

        public async Task<Domains.People.Models.Company> SaveCompany(Domains.People.Models.Company company)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                await db.Database.BeginTransactionAsync();
                if (company.Id == default)
                    company.Id = Guid.NewGuid();
                var entity = await db.Companies.SingleOrDefaultAsync(c => c.Id == company.Id);
                var add = false;
                if (entity == null)
                {
                    entity = new Company {Id = company.Id};
                    add = true;
                }
                entity.CompanyName = company.CompanyName;
                if (company.EmailAddress != null)
                {
                    if (company.EmailAddress.Id == default) company.EmailAddress.Id = Guid.NewGuid();
                    if (entity.EmailAddress != null && company.EmailAddress.Id != entity.EmailAddress.Id)
                    {
                        db.EmailAddresses.Remove(entity.EmailAddress);
                        entity.EmailAddress = null;
                        await db.SaveChangesAsync();
                    }
                    if (entity.EmailAddress == null)
                    {
                        entity.EmailAddress = new EmailAddress {Id = company.EmailAddress.Id};
                        await db.EmailAddresses.AddAsync(entity.EmailAddress);
                    }
                    Mapper.Map(company.EmailAddress, entity.EmailAddress);
                }
                else if (entity.EmailAddress != null)
                {
                    db.EmailAddresses.Remove(entity.EmailAddress);
                    await db.SaveChangesAsync();
                }
                if (company.PhoneNumber != null)
                {
                    if (company.PhoneNumber.Id == default) company.PhoneNumber.Id = Guid.NewGuid();
                    if (entity.PhoneNumber != null && entity.PhoneNumber.Id != company.PhoneNumber.Id)
                    {
                        db.PhoneNumbers.Remove(entity.PhoneNumber);
                        await db.SaveChangesAsync();
                    }
                    if (entity.PhoneNumber == null)
                    {
                        entity.PhoneNumber = new PhoneNumber {Id = company.PhoneNumber.Id};
                        await db.PhoneNumbers.AddAsync(entity.PhoneNumber);
                    }
                    Mapper.Map(company.PhoneNumber, entity.PhoneNumber);
                }
                else if (entity.PhoneNumber != null)
                {
                    db.PhoneNumbers.Remove(entity.PhoneNumber);
                    await db.SaveChangesAsync();
                }
                if (company.OnlinePresence != null)
                {
                    if (company.OnlinePresence.Id == default) company.OnlinePresence.Id = Guid.NewGuid();
                    if (entity.OnlinePresence != null && company.OnlinePresence.Id != entity.OnlinePresence.Id)
                    {
                        db.OnlinePresence.Remove(entity.OnlinePresence);
                        await db.SaveChangesAsync();
                    }
                    if (entity.OnlinePresence == null)
                    {
                        entity.OnlinePresence = new OnlinePresence {Id = company.OnlinePresence.Id};
                        await db.OnlinePresence.AddAsync(entity.OnlinePresence);
                    }
                    Mapper.Map(company.OnlinePresence, entity.OnlinePresence);
                }
                else if (entity.OnlinePresence != null)
                {
                    db.OnlinePresence.Remove(entity.OnlinePresence);
                    await db.SaveChangesAsync();
                }
                company.Locations ??= new List<Domains.People.Models.Location>();
                company.Locations.ForEach(async location =>
                {
                    var saved = await _locations.SaveLocation(location);
                    await db.Entry(entity).ReloadAsync();
                    entity.Locations.Add(db.Locations.Single(l => l.Id == saved.Id));
                    await db.SaveChangesAsync();
                });
                entity.Locations.ToList().ForEach(location =>
                {
                    if (company.Locations.All(l => l.Id != location.Id))
                        _locations.RemoveLocation(location.Id);
                });
                if (add) await db.Companies.AddAsync(entity);
                await db.SaveChangesAsync();
                await db.Database.CommitTransactionAsync();
                return await FindCompanyById(company.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save company data");
                throw new PeopleException("Unable to save company data", ex);
            }
        }

        public async Task<IList<Domains.People.Models.Company>> FindAllCompanies()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.Companies.ToListAsync();
                return entities.Select(MapCompany).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve company data");
                throw new PeopleException("Unable to retrieve company data", ex);
            }
        }

        public async Task<Domains.People.Models.Company> FindCompanyById(Guid companyId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
                return entity == null ? null : MapCompany(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to retrieve company data");
                throw new PeopleException("Unable to retrieve company data", ex);
            }
        }

        public async Task<bool> RemoveCompany(Guid companyId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
                if (entity == null) return false;
                if (entity.Locations != null)
                {
                    foreach (var location in entity.Locations.ToList())
                    {
                        if (location.MailingAddress != null)
                            db.Addresses.Remove(location.MailingAddress);
                        if (location.StreetAddress != null)
                            db.Addresses.Remove(location.StreetAddress);
                        if (location.Contacts != null)
                        {
                            foreach (var lc in location.Contacts.ToList())
                            {
                                db.People.Remove(lc.Contact);
                                db.LocationContacts.Remove(lc);
                            }
                        }
                    }
                }
                db.Companies.Remove(entity);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to remove company data");
                throw new PeopleException("Unable to remove company data", ex);
            }
        }
    }
}
