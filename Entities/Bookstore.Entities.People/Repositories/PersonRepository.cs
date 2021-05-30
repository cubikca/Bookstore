using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Repositories;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Bookstore.Entities.People.Repositories
{
    public class PersonRepository : RepositoryBase, IPersonRepository
    {
        private readonly ILogger<PersonRepository> _logger;
        private readonly IAddressRepository _addresses;

        public PersonRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, IAddressRepository addresses, ILogger<PersonRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
            _addresses = addresses;
        }
        public async Task<Domains.People.Models.Person> SavePerson(Domains.People.Models.Person person)
        {
            try
            {
                if (person.GivenNames == null) throw new ArgumentNullException(nameof(person.GivenNames));
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                if (person.Id == default)
                    person.Id = Guid.NewGuid();
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == person.Id);
                var add = false;
                if (entity == null)
                {
                    entity = new Person {Id = person.Id};
                    add = true;
                }
                entity.Title = person.Title;
                entity.FamilyName = person.FamilyName;
                entity.Suffix = person.Suffix;
                entity.GivenNames ??= new List<PersonGivenName>();
                entity.Initial = person.Initial;
                entity.KnownAs ??= new List<PersonKnownAsName>();
                // order matters, and duplicates are possible so a for loop makes this easier
                for (var i = 0; i < person.GivenNames.Count; i++)
                {
                    // we're going to reuse PersonGivenNames records to keep the id field intact as much as possible
                    if (entity.GivenNames.Count > i)
                    {
                        var pgn = entity.GivenNames.ElementAt(i);
                        pgn.GivenName = person.GivenNames[i];
                    }
                    else
                    {
                        var pgn = new PersonGivenName {Id = Guid.NewGuid(), GivenName = person.GivenNames[i], Person = entity};
                        await db.PersonGivenNames.AddAsync(pgn);
                    }
                }
                if (entity.GivenNames.Count > person.GivenNames.Count)
                {
                    for (var i = person.GivenNames.Count; i < entity.GivenNames.Count; i++)
                    {
                        var pgn = entity.GivenNames.ElementAt(i);
                        db.PersonGivenNames.Remove(pgn);
                    }
                }
                for (var i = 0; i < person.KnownAs.Count; i++)
                {
                    // we're going to reuse PersonKnownAsNames records to keep the id field intact as much as possible
                    if (entity.KnownAs.Count > i)
                    {
                        var aka = entity.KnownAs.ElementAt(i);
                        aka.KnownAsName = person.KnownAs[i];
                    }
                    else
                    {
                        var aka = new PersonKnownAsName {Id = Guid.NewGuid(), KnownAsName = person.KnownAs[i], Person = entity};
                        await db.PersonKnownAsNames.AddAsync(aka);
                    }
                }
                if (entity.KnownAs.Count > person.KnownAs.Count)
                {
                    for (var i = person.KnownAs.Count; i < entity.KnownAs.Count; i++)
                    {
                        var pgn = entity.KnownAs.ElementAt(i);
                        db.PersonKnownAsNames.Remove(pgn);
                    }
                }
                if (person.MailingAddress != null)
                {
                    if (entity.MailingAddress != null && entity.MailingAddress.Id != person.MailingAddress.Id)
                        person.MailingAddress.Id = entity.MailingAddress.Id;
                    person.MailingAddress = await _addresses.SaveAddress(person.StreetAddress);
                }
                else if (entity.MailingAddress != null) // && person.MailingAddress == null
                    await _addresses.RemoveAddress(entity.MailingAddress.Id);
                if (person.StreetAddress != null)
                {
                    if (entity.StreetAddress != null && entity.StreetAddress.Id != person.StreetAddress.Id)
                        person.StreetAddress.Id = entity.StreetAddress.Id;
                    person.StreetAddress = await _addresses.SaveAddress(person.StreetAddress);
                }
                else if (entity.StreetAddress != null)
                    await _addresses.RemoveAddress(entity.StreetAddress.Id);
                if (person.EmailAddress != null)
                {
                    if (entity.EmailAddress != null && entity.EmailAddress.Id != person.EmailAddress.Id)
                    {
                        db.EmailAddresses.Remove(entity.EmailAddress);
                        await db.SaveChangesAsync();
                    }
                    if (entity.EmailAddress == null)
                    {
                        entity.EmailAddress = new EmailAddress {Id = person.EmailAddress.Id};
                        await db.EmailAddresses.AddAsync(entity.EmailAddress);
                    }
                    Mapper.Map(person.EmailAddress, entity.EmailAddress);
                }
                else if (entity.EmailAddress != null)
                {
                    db.EmailAddresses.Remove(entity.EmailAddress);
                    await db.SaveChangesAsync();
                }
                if (person.PhoneNumber != null)
                {
                    if (entity.PhoneNumber != null && entity.PhoneNumber.Id != person.PhoneNumber.Id)
                    {
                        db.PhoneNumbers.Remove(entity.PhoneNumber);
                        await db.SaveChangesAsync();
                    }
                    if (entity.PhoneNumber == null)
                    {
                        entity.PhoneNumber = new PhoneNumber {Id = person.PhoneNumber.Id};
                        await db.PhoneNumbers.AddAsync(entity.PhoneNumber);
                    }
                    Mapper.Map(person.PhoneNumber, entity.PhoneNumber);
                }
                else if (entity.PhoneNumber != null)
                {
                    db.PhoneNumbers.Remove(entity.PhoneNumber);
                    await db.SaveChangesAsync();
                }
                if (person.OnlinePresence != null)
                {
                    if (entity.OnlinePresence != null && entity.OnlinePresence.Id != person.OnlinePresence.Id)
                    {
                        db.OnlinePresence.Remove(entity.OnlinePresence);
                        await db.SaveChangesAsync();
                    }
                    if (entity.OnlinePresence == null)
                    {
                        entity.OnlinePresence = new OnlinePresence {Id = person.OnlinePresence.Id};
                        await db.OnlinePresence.AddAsync(entity.OnlinePresence);
                    }
                    Mapper.Map(person.OnlinePresence, entity.OnlinePresence);
                }
                else if (entity.OnlinePresence != null)
                {
                    db.OnlinePresence.Remove(entity.OnlinePresence);
                    await db.SaveChangesAsync();
                }
                if (add) await db.People.AddAsync(entity);
                await db.SaveChangesAsync();
                person = await FindPersonById(person.Id);
                scope.Complete();
                return person;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save person data");
                throw new PeopleException("Unable to save person data", ex);
            }
        }

        public async Task<ICollection<Domains.People.Models.Person>> FindAllPeople()
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entities = await db.People.ToListAsync();
                var people = entities.Select(MapPerson).ToList();
                return people;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to retrieve person data", ex);
                throw new PeopleException("Unable to retrieve person data", ex);
            }
        }

        public async Task<Domains.People.Models.Person> FindPersonById(Guid personId)
        {
            try
            {
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == personId);
                if (entity == null) return null;
                var person = MapPerson(entity);
                return person;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to retrieve person data", ex);
                throw new PeopleException("Unable to retrieve person data", ex);
            }
        }

        public async Task<bool> RemovePerson(Guid personId)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.People.SingleOrDefaultAsync(p => p.Id == personId);
                if (entity == null) return false;
                if (entity.MailingAddress != null)
                    db.Addresses.Remove(entity.MailingAddress);
                if (entity.StreetAddress != null)
                    db.Addresses.Remove(entity.StreetAddress);
                if (entity.PhoneNumber != null)
                    db.PhoneNumbers.Remove(entity.PhoneNumber);
                if (entity.EmailAddress != null)
                    db.EmailAddresses.Remove(entity.EmailAddress);
                if (entity.OnlinePresence != null)
                    db.OnlinePresence.Remove(entity.OnlinePresence);
                db.People.Remove(entity);
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to remove person data", ex);
                throw new PeopleException("Unable to remove person data", ex);
            }
        }
    }
}
