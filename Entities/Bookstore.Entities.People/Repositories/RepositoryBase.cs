using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bookstore.Entities.People.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Entities.People.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly IDbContextFactory<PeopleContext> DbFactory;
        protected readonly IMapper Mapper;

        protected RepositoryBase(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
        }

        protected Domains.People.Models.EmailAddress MapEmailAddress(EmailAddress email)
        {
            return email != null 
                ? Mapper.Map<Domains.People.Models.EmailAddress>(email) 
                : null;
        }

        protected Domains.People.Models.Address MapAddress(Address address)
        {
            return address != null
                ? Mapper.Map<Domains.People.Models.Address>(address)
                : null;
        }

        protected Domains.People.Models.Location MapLocation(Location location)
        {
            var model = new Domains.People.Models.Location
            {
                Id = location.Id,
                CompanyId = location.CompanyId,
                MailingAddress = MapAddress(location.MailingAddress),
                StreetAddress = MapAddress(location.StreetAddress),
                Primary = location.Primary,
                Contacts = new List<Domains.People.Models.Person>()
            };
            foreach (var contact in location.Contacts.ToList())
                model.Contacts.Add(MapPerson(contact.Contact)); 
            return model;
        }

        protected Domains.People.Models.OnlinePresence MapOnlinePresence(OnlinePresence onlinePresence)
        {
            return onlinePresence != null
                ? Mapper.Map<Domains.People.Models.OnlinePresence>(onlinePresence)
                : null;
        }

        protected Domains.People.Models.PhoneNumber MapPhoneNumber(PhoneNumber phone)
        {
            return phone != null
                ? Mapper.Map<Domains.People.Models.PhoneNumber>(phone)
                : null;
        }

        protected Domains.People.Models.Person MapPerson(Person person)
        {
            if (person == null) return null;
            var model = new Domains.People.Models.Person
            {
                Id = person.Id,
                FamilyName = person.FamilyName,
                EmailAddress = MapEmailAddress(person.EmailAddress),
                Initial = person.Initial,
                Title = person.Title,
                Suffix = person.Suffix,
                StreetAddress = MapAddress(person.StreetAddress),
                MailingAddress = MapAddress(person.MailingAddress),
                OnlinePresence = MapOnlinePresence(person.OnlinePresence),
                GivenNames = person.GivenNames.Select(n => n.GivenName).ToList(),
                KnownAs = person.KnownAs.Select(n => n.KnownAsName).ToList(),
                PhoneNumber = MapPhoneNumber(person.PhoneNumber)
            };
            return model;
        }
        protected Domains.People.Models.Company MapCompany(Company company)
        {
            if (company == null) return null;
            company.Locations ??= new List<Location>();
            var model = new Domains.People.Models.Company
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                EmailAddress = MapEmailAddress(company.EmailAddress),
                PhoneNumber = MapPhoneNumber(company.PhoneNumber),
                OnlinePresence = MapOnlinePresence(company.OnlinePresence),
                Locations = company.Locations.Select(MapLocation).ToList()
            };
            return model;
        }
    }
}
