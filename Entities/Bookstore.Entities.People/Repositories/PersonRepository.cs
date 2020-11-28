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

namespace Bookstore.Entities.People.Repositories
{
    public class PersonRepository : RepositoryBase, IPersonRepository
    {
        private readonly ILogger<PersonRepository> _logger;

        public PersonRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<PersonRepository> logger) : base(dbFactory, mapper)
        {
            _logger = logger;
        }
        public async Task<Domains.People.Models.Person> SavePerson(Domains.People.Models.Person person)
        {
            try
            {
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
                foreach (var name in person.GivenNames)
                {
                    if (entity.GivenNames.All(n => n.GivenName != name))
                    {
                        var pgn = new PersonGivenName {Id = Guid.NewGuid(), PersonId = entity.Id, GivenName = name};
                        await db.PersonGivenNames.AddAsync(pgn);
                        entity.GivenNames.Add(pgn);
                    }
                    else
                    {
                        var pgn = entity.GivenNames.SingleOrDefault(n => n.GivenName == name);
                        if (pgn == null) continue;
                        db.PersonGivenNames.Remove(pgn);
                    }
                }
                foreach (var name in person.KnownAs)
                {
                    if (entity.KnownAs.All(n => n.KnownAsName != name))
                    {
                        var paka = new PersonKnownAsName {Id = Guid.NewGuid(), PersonId = entity.Id, KnownAsName = name};
                        await db.PersonKnownAsNames.AddAsync(paka);
                        entity.KnownAs.Add(paka);
                    }
                    else
                    {
                        var paka = entity.KnownAs.SingleOrDefault(n => n.KnownAsName == name);
                        if (paka == null) continue;
                        db.PersonKnownAsNames.Remove(paka);
                    }
                }
                if (person.MailingAddress != null)
                {
                    entity.MailingAddress ??= new Address {Id = person.MailingAddress.Id};
                    Mapper.Map(person.MailingAddress, entity.MailingAddress);
                }
                else if (entity.MailingAddress != null) // && person.MailingAddress == null
                {
                    db.Addresses.Remove(entity.MailingAddress);
                    entity.MailingAddress = null;
                }
                if (person.StreetAddress != null)
                {
                    entity.StreetAddress ??= new Address {Id = person.StreetAddress.Id};
                    Mapper.Map(person.MailingAddress, entity.MailingAddress);
                }
                else if (entity.StreetAddress != null)
                {
                    db.Addresses.Remove(entity.StreetAddress);
                    entity.StreetAddress = null;
                }
                if (person.EmailAddress != null)
                {
                    entity.EmailAddress ??= new EmailAddress {Id = person.EmailAddress.Id};
                    Mapper.Map(person.EmailAddress, entity.EmailAddress);
                }
                else if (entity.EmailAddress != null)
                {
                    db.EmailAddresses.Remove(entity.EmailAddress);
                    entity.EmailAddress = null;
                }
                if (person.PhoneNumber != null)
                {
                    entity.PhoneNumber ??= new PhoneNumber {Id = person.PhoneNumber.Id};
                    Mapper.Map(person.PhoneNumber, entity.PhoneNumber);
                }
                else if (entity.PhoneNumber != null)
                {
                    db.PhoneNumbers.Remove(entity.PhoneNumber);
                    entity.PhoneNumber = null;
                }
                if (person.OnlinePresence != null)
                {
                    entity.OnlinePresence ??= new OnlinePresence {Id = person.OnlinePresence.Id};
                    Mapper.Map(person.OnlinePresence, entity.OnlinePresence);
                }
                else if (entity.OnlinePresence != null)
                {
                    db.OnlinePresence.Remove(entity.OnlinePresence);
                    entity.OnlinePresence = null;
                }
                if (add) await db.People.AddAsync(entity);
                await db.SaveChangesAsync();
                person = await FindPersonById(person.Id);
                return person;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save person data");
                throw new PeopleException("Unable to save person data", ex);
            }
        }

        public async Task<IList<Domains.People.Models.Person>> FindAllPeople()
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
