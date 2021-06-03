using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Location = Bookstore.Domains.People.Models.Location;
using Person = Bookstore.Domains.People.Models.Person;

namespace Bookstore.Entities.People.Repositories
{
    public class LocationRepository : RepositoryBase<Location, Models.Location>, ILocationRepository
    {
        private readonly IPersonRepository _people;
        private readonly IAddressRepository _addresses;
        
        public LocationRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IPersonRepository people, IAddressRepository addresses, ILogger<RepositoryBase<Location, Models.Location>> logger) : base(dbFactory, mapper, logger)
        {
            _people = people;
            _addresses = addresses;
        }

        private async Task SaveContacts(PeopleContext db, Models.Location entity, ICollection<Person> contacts)
        {
            foreach (var person in contacts)
            {
                var saved = await _people.Save(person);
                var locationContact = new LocationContact
                {
                    Contact = await db.People.SingleAsync(p => p.Id == saved.Id),
                    ContactId = person.Id,
                    Location = entity,
                    LocationId = entity.Id
                };
                await db.LocationContacts.AddAsync(locationContact);
            }
            await db.SaveChangesAsync();
            foreach (var locationContact in entity.Contacts.ToList())
            {
                if (contacts.All(c => c.Id != locationContact.ContactId))
                    db.LocationContacts.Remove(locationContact);
            }
            await db.SaveChangesAsync();
        }

        private async Task RemoveContacts(PeopleContext db, Models.Location entity)
        {
            foreach (var locationContact in entity.Contacts.ToList())
            {
                db.LocationContacts.Remove(locationContact);
                await db.SaveChangesAsync();
                await _people.Remove(locationContact.ContactId);
            }
        }

        public override async Task<Location> Save(Location model)
        {
            using var scope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await using var db = DbFactory.CreateDbContext();
            var location = await base.Save(model);
            var entity = await db.Locations.SingleAsync(l => l.Id == location.Id);
            if (model.StreetAddress != null)
            {
                var address = await _addresses.Save(model.StreetAddress);
                entity.StreetAddress = await db.Addresses.SingleAsync(a => a.Id == address.Id);
                entity.StreetAddressId = address.Id;
                await db.SaveChangesAsync();
            }
            else if (entity.StreetAddress != null)
            {
                await _addresses.Remove(entity.StreetAddress.Id);
                entity.StreetAddress = null;
                entity.StreetAddressId = null;
                await db.SaveChangesAsync();
            }
            if (model.MailingAddress != null)
            {
                var address = await _addresses.Save(model.MailingAddress);
                entity.MailingAddress = await db.Addresses.SingleAsync(a => a.Id == address.Id);
                entity.MailingAddressId = address.Id;
                await db.SaveChangesAsync();
            }
            else if (entity.MailingAddress != null)
            {
                await _addresses.Remove(entity.MailingAddress.Id);
                entity.MailingAddress = null;
                entity.MailingAddressId = null;
                await db.SaveChangesAsync();
            }
            await SaveContacts(db, entity, model.Contacts);
            await db.SaveChangesAsync();
            var result = await Find(model.Id);
            scope.Complete();
            return result;
        }

        public override async Task<bool> Remove(Guid id)
        {
            using var scope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await using var db = DbFactory.CreateDbContext();
            var entity = await db.Locations.SingleOrDefaultAsync(l => l.Id == id);
            if (entity == null) return false;
            await RemoveContacts(db, entity);
            var result = await base.Remove(id);
            scope.Complete();
            return result;
        }
    }
}