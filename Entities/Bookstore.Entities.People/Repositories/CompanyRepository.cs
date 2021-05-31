using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
using Person = Bookstore.Domains.People.Models.Person;

namespace Bookstore.Entities.People.Repositories
{
    public class CompanyRepository : RepositoryBase, ICompanyRepository
    {
        private readonly ILogger<CompanyRepository> _logger;
        private readonly IAddressRepository _addresses;
        private readonly IPersonRepository _people;

        public CompanyRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IAddressRepository addresses, IPersonRepository people, ILogger<CompanyRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
            _addresses = addresses;
            _people = people;
        }

        private async Task SetCompanyEmail(PeopleContext db, Company entity, Domains.People.Models.Company model)
        {
            if (model.EmailAddress != null)
            {
                if (model.EmailAddress.Id == default) model.EmailAddress.Id = Guid.NewGuid();
                if (entity.EmailAddress != null && model.EmailAddress.Id != entity.EmailAddress.Id)
                {
                    db.EmailAddresses.Remove(entity.EmailAddress);
                    entity.EmailAddress = null;
                    await db.SaveChangesAsync();
                }

                if (entity.EmailAddress == null)
                {
                    entity.EmailAddress = new EmailAddress {Id = model.EmailAddress.Id};
                    await db.EmailAddresses.AddAsync(entity.EmailAddress);
                }

                Mapper.Map(model.EmailAddress, entity.EmailAddress);
            }
            else if (entity.EmailAddress != null)
            {
                db.EmailAddresses.Remove(entity.EmailAddress);
                await db.SaveChangesAsync();
            }
        }

        private async Task SetCompanyPhone(PeopleContext db, Company entity, Domains.People.Models.Company model)
        {
             if (model.PhoneNumber != null)
             {
                 if (model.PhoneNumber.Id == default) model.PhoneNumber.Id = Guid.NewGuid();
                 if (entity.PhoneNumber != null && entity.PhoneNumber.Id != model.PhoneNumber.Id)
                 {
                     db.PhoneNumbers.Remove(entity.PhoneNumber);
                     await db.SaveChangesAsync();
                 }
                 if (entity.PhoneNumber == null)
                 {
                     entity.PhoneNumber = new PhoneNumber {Id = model.PhoneNumber.Id};
                     await db.PhoneNumbers.AddAsync(entity.PhoneNumber);
                 }
                 Mapper.Map(model.PhoneNumber, entity.PhoneNumber);
             }
             else if (entity.PhoneNumber != null)
             {
                 db.PhoneNumbers.Remove(entity.PhoneNumber);
                 await db.SaveChangesAsync();
             }
        }

        private async Task SetCompanyOnlinePresence(PeopleContext db, Company entity,
            Domains.People.Models.Company model)
        {
             if (model.OnlinePresence != null)
             {
                 if (model.OnlinePresence.Id == default) model.OnlinePresence.Id = Guid.NewGuid();
                 if (entity.OnlinePresence != null && model.OnlinePresence.Id != entity.OnlinePresence.Id)
                 {
                     db.OnlinePresence.Remove(entity.OnlinePresence);
                     await db.SaveChangesAsync();
                 }
                 if (entity.OnlinePresence == null)
                 {
                     entity.OnlinePresence = new OnlinePresence {Id = model.OnlinePresence.Id};
                     await db.OnlinePresence.AddAsync(entity.OnlinePresence);
                 }
                 Mapper.Map(model.OnlinePresence, entity.OnlinePresence);
             }
             else if (entity.OnlinePresence != null)
             {
                 db.OnlinePresence.Remove(entity.OnlinePresence);
                 await db.SaveChangesAsync();
             }
        }

        private async Task SetLocationContacts(PeopleContext db, Location entity, Domains.People.Models.Location model)
        {
            model.Contacts ??= new List<Person>();
            foreach (var locationContact in model.Contacts)
            {
                var savedContact = await _people.SavePerson(locationContact);
                var lc = new LocationContact {ContactId = savedContact.Id, LocationId = entity.Id};
                if (db.LocationContacts.All(x => x.ContactId != lc.ContactId || x.LocationId != lc.LocationId))
                    await db.LocationContacts.AddAsync(lc);
            }
            foreach (var locationContact in entity.Contacts.ToList())
            {
                // remove LocationContacts not in input list
                if (model.Contacts.All(c => c.Id != locationContact.ContactId))
                {
                    db.LocationContacts.Remove(locationContact);
                    await db.SaveChangesAsync();
                }
                // cannot clean up orphans because we have no way of knowing whether the orphans are in use
                // because other domains can reference this one from another database context
            }
        }
        
        private async Task SetCompanyLocations(PeopleContext db, Company entity, Domains.People.Models.Company model)
        {
             model.Locations ??= new List<Domains.People.Models.Location>();
             var tasks = model.Locations.Select(async location =>
             {
                 var locationEntity = await db.Locations.SingleOrDefaultAsync(l => l.Id == location.Id);
                 if (locationEntity == null)
                 {
                     locationEntity = new Location {Id = location.Id, CompanyId = entity.Id};
                     await db.Locations.AddAsync(locationEntity);
                     await db.SaveChangesAsync();
                 }
                 Mapper.Map(location, locationEntity);
                 if (location.MailingAddress != null)
                 {
                     var saved = await _addresses.SaveAddress(location.MailingAddress);
                     locationEntity.MailingAddressId = saved.Id;
                 }
                 if (location.StreetAddress != null)
                 {
                     var saved = await _addresses.SaveAddress(location.StreetAddress);
                     locationEntity.StreetAddressId = saved.Id;
                 }
                 await SetLocationContacts(db, locationEntity, location);
             });
             await Task.WhenAll(tasks);
             // remove locations not in the model
             foreach (var location in entity.Locations)
             {
                 if (model.Locations.All(l => l.Id != location.Id))
                     entity.Locations.Remove(location);
             }
        }

        public async Task<Domains.People.Models.Company> SaveCompany(Domains.People.Models.Company company)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
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
                await SetCompanyEmail(db, entity, company);
                await SetCompanyPhone(db, entity, company);
                await SetCompanyOnlinePresence(db, entity, company);
                if (add) await db.Companies.AddAsync(entity);
                await SetCompanyLocations(db, entity, company);
                await db.SaveChangesAsync();
                var result = await FindCompanyById(company.Id);
                scope.Complete();
                return result;
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
