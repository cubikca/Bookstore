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

        private async Task SaveGivenNames(PeopleContext db, Models.Person entity, IList<string> names)
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
                    await db.PersonGivenNames.AddAsync(gn);
                }
            }
            if (entity.GivenNames.Count > names.Count)
            {
                for (var i = names.Count; i < entity.GivenNames.Count; i++)
                    db.PersonGivenNames.Remove(entity.GivenNames[i]);
            }
            await db.SaveChangesAsync();
        }

        private async Task SaveKnownAsNames(PeopleContext db, Models.Person entity, IList<string> names)
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
                     await db.PersonKnownAsNames.AddAsync(aka);
                 }
             }
             if (entity.KnownAs.Count > names.Count)
             {
                 for (var i = names.Count; i < entity.KnownAs.Count; i++)
                     db.PersonKnownAsNames.Remove(entity.KnownAs[i]);
             }
             await db.SaveChangesAsync();           
        }

        public override async Task<Person> Save(Person model)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var person = await base.Save(model);
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == person.Id);
                await SaveGivenNames(db, entity, model.GivenNames);
                await SaveKnownAsNames(db, entity, model.KnownAs);
                if (model.StreetAddress != null)
                {
                    if (entity.StreetAddress != null)
                        await _addresses.Remove(entity.StreetAddress.Id);
                    var address = await _addresses.Save(model.StreetAddress);
                    entity.StreetAddress = await db.Addresses.SingleOrDefaultAsync(a => a.Id == address.Id);
                    entity.StreetAddressId = address.Id;
                    await db.SaveChangesAsync();
                }
                if (model.MailingAddress != null)
                {
                    if (entity.MailingAddress != null)
                        await _addresses.Remove(entity.MailingAddress.Id);
                    var address = await _addresses.Save(model.MailingAddress);
                    entity.MailingAddress = await db.Addresses.SingleOrDefaultAsync(a => a.Id == address.Id);
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
                var result = await Find(person.Id);
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
    }
}