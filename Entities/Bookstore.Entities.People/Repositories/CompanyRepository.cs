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

        public CompanyRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<CompanyRepository> logger, IPersonRepository people) : base(dbFactory, mapper)
        {
            _logger = logger;
            _people = people;
        }

        private async Task SaveLocation(PeopleContext db, Company entity, Domains.People.Models.Location location)
        {
            var locationEntity = await db.Locations.SingleOrDefaultAsync(l => l.Id == location.Id) 
                                 ?? new Location {Id = location.Id, Company = entity};
            if (location.MailingAddress != null)
            {
                locationEntity.MailingAddress ??= new Address();
                Mapper.Map(location.MailingAddress, locationEntity.MailingAddress);
            }
            else if (locationEntity.MailingAddress != null)
            {
                db.Addresses.Remove(locationEntity.MailingAddress);
                locationEntity.MailingAddress = null;
            }
            if (location.StreetAddress != null)
            {
                locationEntity.StreetAddress ??= new Address();
                Mapper.Map(location.StreetAddress, locationEntity.StreetAddress);
            }
            else if (locationEntity.StreetAddress != null)
            {
                db.Addresses.Remove(locationEntity.StreetAddress);
                locationEntity.StreetAddress = null;
            }
            location.Contacts ??= new List<Domains.People.Models.Person>();
            location.Contacts.ForEach(contact =>
            {
                contact = _people.SavePerson(contact).GetAwaiter().GetResult();
                var locationContact = new LocationContact {ContactId = contact.Id, Location = locationEntity};
                locationEntity.Contacts ??= new List<LocationContact>();
                db.LocationContacts.Add(locationContact);
                locationEntity.Contacts.Add(locationContact);
            });
            locationEntity.Primary = location.Primary;
            if (entity.Locations.All(l => l.Id != locationEntity.Id))
            {
                await db.Locations.AddAsync(locationEntity);
                entity.Locations.Add(locationEntity);
            }
            foreach (var contact in locationEntity.Contacts.ToList().Where(contact => location.Contacts.All(c => c.Id != contact.ContactId)))
            {
                db.People.Remove(contact.Contact);
                db.LocationContacts.Remove(contact);
            }
        }

        public async Task<Domains.People.Models.Company> SaveCompany(Domains.People.Models.Company company)
        {
            try
            {
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
                if (company.EmailAddress != null)
                {
                    if (company.EmailAddress.Id == default) company.EmailAddress.Id = Guid.NewGuid();
                    entity.EmailAddress ??= new EmailAddress {Id = company.EmailAddress.Id};
                    Mapper.Map(company.EmailAddress, entity.EmailAddress);
                }
                else if (entity.EmailAddress != null)
                {
                    db.EmailAddresses.Remove(entity.EmailAddress);
                    entity.EmailAddress = null;
                }
                if (company.PhoneNumber != null)
                {
                    if (company.PhoneNumber.Id == default) company.PhoneNumber.Id = Guid.NewGuid();
                    entity.PhoneNumber ??= new PhoneNumber {Id = company.PhoneNumber.Id};
                    Mapper.Map(company.PhoneNumber, entity.PhoneNumber);
                }
                else if (entity.PhoneNumber != null)
                {
                    db.PhoneNumbers.Remove(entity.PhoneNumber);
                    entity.PhoneNumber = null;
                }
                if (company.OnlinePresence != null)
                {
                    if (company.OnlinePresence.Id == default) company.OnlinePresence.Id = Guid.NewGuid();
                    entity.OnlinePresence ??= new OnlinePresence();
                    Mapper.Map(company.OnlinePresence, entity.OnlinePresence);
                }
                else if (entity.OnlinePresence != null)
                {
                    db.OnlinePresence.Remove(entity.OnlinePresence);
                    Mapper.Map(company.OnlinePresence, entity.OnlinePresence);
                }
                company.Locations ??= new List<Domains.People.Models.Location>();
                company.Locations.ForEach(location => SaveLocation(db, entity, location).Wait());
                entity.Locations.ToList().ForEach(location =>
                {
                    if (company.Locations.All(l => l.Id != location.Id))
                        db.Locations.Remove(location);
                });
                if (add) await db.Companies.AddAsync(entity);
                await db.SaveChangesAsync();
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
