using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using EmailAddress = Bookstore.Domains.People.Models.EmailAddress;
using OnlinePresence = Bookstore.Domains.People.Models.OnlinePresence;
using Person = Bookstore.Domains.People.Models.Person;
using PhoneNumber = Bookstore.Domains.People.Models.PhoneNumber;

namespace Bookstore.Entities.People.Repositories
{
    public class PersonRepository : RepositoryBase<Person, Models.Person>, IPersonRepository
    {
        private readonly IAddressRepository _addresses;
        
        public PersonRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IAddressRepository addresses, ILogger<RepositoryBase<Person, Models.Person>> logger) : base(dbFactory, mapper, logger)
        {
            _addresses = addresses;
        }

        private async Task SaveEmail(Models.Person entity, [NotNull] EmailAddress email)
        {
            await using var db = DbFactory.CreateDbContext();
            if (entity.EmailAddress != null && entity.EmailAddressId != email.Id)
                await RemoveEmail(entity);
            if (entity.EmailAddressId == null)
            {
                var emailEntity = new Models.EmailAddress {Id = email.Id};
                await db.EmailAddresses.AddAsync(emailEntity);
                entity.EmailAddress = emailEntity;
            }
            Mapper.Map(email, entity.EmailAddress);
            entity.EmailAddressId = email.Id;
            await db.SaveChangesAsync();
        }

        private async Task RemoveEmail(Models.Person entity)
        {
            await using var db = DbFactory.CreateDbContext();
            entity.EmailAddress.Deleted = true;
            entity.EmailAddress = null;
            entity.EmailAddressId = null;
            await db.SaveChangesAsync();
        }

        private async Task SaveOnlinePresence(Models.Person entity, [NotNull] OnlinePresence onlinePresence)
        {
            await using var db = DbFactory.CreateDbContext();
            if (entity.OnlinePresence != null && entity.OnlinePresenceId != onlinePresence.Id)
                await RemoveOnlinePresence(entity);
            if (entity.OnlinePresenceId == null)
            {
                var onlinePresenceEntity = new Models.OnlinePresence {Id = onlinePresence.Id};
                await db.OnlinePresence.AddAsync(onlinePresenceEntity);
                entity.OnlinePresence = onlinePresenceEntity;
                entity.OnlinePresenceId = onlinePresence.Id;
            }
            Mapper.Map(onlinePresence, entity.OnlinePresence);
            await db.SaveChangesAsync();
        }

        private async Task RemoveOnlinePresence(Models.Person entity)
        {
            await using var db = DbFactory.CreateDbContext();
            entity.OnlinePresence.Deleted = true;
            entity.OnlinePresence = null;
            entity.OnlinePresenceId = null;
            await db.SaveChangesAsync();
        }

        private async Task SavePhoneNumber(Models.Person entity, [NotNull] PhoneNumber phone)
        {
            await using var db = DbFactory.CreateDbContext();
            if (entity.PhoneNumber != null && entity.PhoneNumberId != phone.Id)
                await RemovePhoneNumber(entity);
            if (entity.PhoneNumberId == null)
            {
                var phoneEntity = new Models.PhoneNumber {Id = phone.Id};
                await db.PhoneNumbers.AddAsync(phoneEntity);
                entity.PhoneNumber = phoneEntity;
            }
            Mapper.Map(phone, entity.PhoneNumber);
            entity.OnlinePresenceId = phone.Id;
            await db.SaveChangesAsync();
        }

        private async Task RemovePhoneNumber(Models.Person entity)
        {
            await using var db = DbFactory.CreateDbContext();
            entity.PhoneNumber.Deleted = true;
            entity.PhoneNumber = null;
            entity.PhoneNumberId = null;
            await db.SaveChangesAsync();
        }

        private void SaveGivenNames(PeopleContext db, Models.Person entity, IList<string> names)
        {
            if (entity.GivenNames == null) entity.GivenNames = new List<PersonGivenName>();
            for (var i = 0; i < names.Count; i++)
            {
                if (i < entity.GivenNames.Count)
                {
                    entity.GivenNames[i].Person = entity;
                    entity.GivenNames[i].PersonId = entity.Id;
                    entity.GivenNames[i].GivenName = names[i];
                }
                else
                {
                    var gn = new PersonGivenName
                    {
                        Person = entity,
                        PersonId = entity.Id,
                        GivenName = names[i]
                    };
                    entity.GivenNames.Add(gn);
                    db.PersonGivenNames.Add(gn);
                }
            }
            if (entity.GivenNames.Count > names.Count)
            {
                for (var i = names.Count; i < entity.GivenNames.Count; i++)
                    db.PersonGivenNames.Remove(entity.GivenNames[i]);
            }
            db.SaveChanges();
        }

        private void SaveKnownAsNames(PeopleContext db, Models.Person entity, IList<string> names)
        {
             if (entity.KnownAs == null) entity.KnownAs = new List<PersonKnownAsName>();
             for (var i = 0; i < names.Count; i++)
             {
                 if (i < entity.KnownAs.Count)
                 {
                     entity.KnownAs[i].Person = entity;
                     entity.KnownAs[i].PersonId = entity.Id;
                     entity.KnownAs[i].KnownAsName = names[i];
                 }
                 else
                 {
                     var aka = new PersonKnownAsName
                     {
                         Person = entity,
                         PersonId = entity.Id,
                         KnownAsName = names[i]
                     };
                     entity.KnownAs.Add(aka);
                     db.PersonKnownAsNames.Add(aka);
                 }
             }
             if (entity.KnownAs.Count > names.Count)
             {
                 for (var i = names.Count; i < entity.KnownAs.Count; i++)
                     db.PersonKnownAsNames.Remove(entity.KnownAs[i]);
             }
             db.SaveChanges();           
        }

        public override async Task<Person> Save(Person model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var person = await base.Save(model);
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == person.Id && !p.Deleted);
                SaveGivenNames(db, entity, model.GivenNames);
                SaveKnownAsNames(db, entity, model.KnownAs);
                if (model.StreetAddress != null)
                {
                    if (entity.StreetAddress != null)
                        await _addresses.Remove(entity.StreetAddress.Id);
                    var address = await _addresses.Save(model.StreetAddress);
                    entity.StreetAddress = await db.Addresses.FindAsync(address.Id);
                    entity.StreetAddressId = address.Id;
                    await db.SaveChangesAsync();
                }
                if (model.MailingAddress != null)
                {
                    if (entity.MailingAddress != null)
                        await _addresses.Remove(entity.MailingAddress.Id);
                    var address = await _addresses.Save(model.MailingAddress);
                    entity.MailingAddress = await db.Addresses.FindAsync(address.Id);
                    entity.MailingAddressId = address.Id;
                    await db.SaveChangesAsync();
                }
                if (model.EmailAddress != null)
                    await SaveEmail(entity, model.EmailAddress);
                else
                    await RemoveEmail(entity);
                if (model.PhoneNumber != null)
                    await SavePhoneNumber(entity, model.PhoneNumber);
                else
                    await RemovePhoneNumber(entity);
                if (model.OnlinePresence != null)
                    await SaveOnlinePresence(entity, model.OnlinePresence);
                else
                    await RemoveOnlinePresence(entity);
                var result = Mapper.Map<Person>(await db.People.SingleOrDefaultAsync(p => p.Id == person.Id && !p.Deleted));
                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                var msg = "Unable to save Entity of type Person";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<Person> Find(Guid id)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == id && !p.Deleted);
                var person = Mapper.Map<Person>(entity);
                return person;
            }
            catch (Exception ex)
            {
                var msg = "Failed to retrieve Entity data of type Person";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<ICollection<Person>> FindAll()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.People
                    .Where(p => !p.Deleted)
                    .ToListAsync();
                var people = Mapper.Map<List<Person>>(entities);
                return people;
            }
            catch (Exception ex)
            {
                var msg = "Failed to retrieve Entity data of type Person";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }

        public override async Task<bool> Remove(Guid id)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == id && !p.Deleted);
                if (entity == null) return false;
                if (entity.GivenNames != null)
                {
                    db.PersonGivenNames.RemoveRange(entity.GivenNames);
                    await db.SaveChangesAsync();
                }
                if (entity.KnownAs != null)
                {
                    db.PersonKnownAsNames.RemoveRange(entity.KnownAs);
                    await db.SaveChangesAsync();
                }
                if (entity.StreetAddressId.HasValue)
                {
                    var addressEntity = await db.Addresses.FindAsync(entity.StreetAddressId);
                    if (addressEntity != null) addressEntity.Deleted = true;
                    entity.StreetAddress = null;
                    entity.StreetAddressId = null;
                    await db.SaveChangesAsync();
                }
                if (entity.MailingAddressId.HasValue)
                {
                    var addressEntity = await db.Addresses.FindAsync(entity.MailingAddressId);
                    if (addressEntity != null) addressEntity.Deleted = true;
                    entity.MailingAddress = null;
                    entity.MailingAddressId = null;
                    await db.SaveChangesAsync();
                }
                if (entity.EmailAddress != null)
                    await RemoveEmail(entity);
                if (entity.PhoneNumber != null)
                    await RemovePhoneNumber(entity);
                if (entity.OnlinePresence != null)
                    await RemoveOnlinePresence(entity);
                entity.Deleted = true;
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                var msg = "Failed to remove Entity of type Person";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }
    }
}