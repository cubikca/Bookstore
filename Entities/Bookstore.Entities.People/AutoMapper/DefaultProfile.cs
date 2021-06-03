using System.Linq;
using AutoMapper;
using Bookstore.Domains.People.Models;
using Bookstore.Entities.People.Models;

namespace Bookstore.Entities.People.AutoMapper
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<IDomainObject, IEntity>()
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.Deleted, opt => opt.Ignore())
                .ForMember(d => d.Created, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.Updated, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.Country, Models.Country>()
                .IncludeBase<IDomainObject,IEntity>()
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.Province, Models.Province>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(p => p.CountryId, opt => opt.Ignore())
                .ForMember(p => p.Country, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.Address, Models.Address>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(a => a.Province, opt => opt.Ignore())
                .ForMember(a => a.Country, opt => opt.Ignore())
                .ForMember(a => a.ProvinceId, opt => opt.Ignore())
                .ForMember(a => a.CountryId, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.EmailAddress, Models.EmailAddress>()
                .IncludeBase<IDomainObject, IEntity>()
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.PhoneNumber, Models.PhoneNumber>()
                .IncludeBase<IDomainObject, IEntity>()
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.OnlinePresence, Models.OnlinePresence>()
                .IncludeBase<IDomainObject, IEntity>()
                .ReverseMap();
            CreateMap<Bookstore.Domains.People.Models.Subject, Models.Subject>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(c => c.Name, opt => opt.Ignore())
                .ForMember(c => c.FullName, opt => opt.Ignore())
                .ForMember(c => c.MailingAddress, opt => opt.Ignore())
                .ForMember(c => c.StreetAddress, opt => opt.Ignore());
            CreateMap<Models.Subject, Bookstore.Domains.People.Models.Subject>()
                .IncludeBase<IEntity, IDomainObject>()
                .ForMember(s => s.StreetAddress, opt => opt.Ignore())
                .ForMember(s => s.MailingAddress, opt => opt.Ignore());
            CreateMap<Bookstore.Domains.People.Models.Person, Models.Person>()
                .IncludeBase<Bookstore.Domains.People.Models.Subject, Models.Subject>()
                .ForMember(p => p.MailingAddress, opt => opt.Ignore())
                .ForMember(p => p.StreetAddress, opt => opt.Ignore())
                .ForMember(p => p.MailingAddressId, opt => opt.Ignore())
                .ForMember(p => p.StreetAddressId, opt => opt.Ignore())
                .ForMember(p => p.EmailAddress, opt => opt.Ignore())
                .ForMember(p => p.PhoneNumber, opt => opt.Ignore())
                .ForMember(p => p.OnlinePresence, opt => opt.Ignore())
                .ForMember(p => p.EmailAddressId, opt => opt.Ignore())
                .ForMember(p => p.PhoneNumberId, opt => opt.Ignore())
                .ForMember(p => p.OnlinePresenceId, opt => opt.Ignore())
                .ForMember(p => p.KnownAs, opt => opt.Ignore())
                .ForMember(p => p.GivenNames, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(p => p.KnownAs, opt => opt.MapFrom(src => src.KnownAs.Select(aka => aka.KnownAsName).ToArray()))
                .ForMember(p => p.GivenNames, opt => opt.MapFrom(src => src.GivenNames.Select(gn => gn.GivenName).ToArray()));
            CreateMap<Bookstore.Domains.People.Models.Location, Models.Location>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(l => l.StreetAddress, opt => opt.Ignore())
                .ForMember(l => l.MailingAddress, opt => opt.Ignore())
                .ForMember(l => l.MailingAddressId, opt => opt.Ignore())
                .ForMember(l => l.StreetAddressId, opt => opt.Ignore())
                .ForMember(l => l.Contacts, opt => opt.Ignore())
                .ForMember(l => l.Company, opt => opt.Ignore())
                .ForMember(l => l.CompanyId, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(l => l.Contacts, opt => opt.MapFrom(src => src.Contacts.Select(lc => lc.Contact)));
            CreateMap<Bookstore.Domains.People.Models.Company, Models.Company>()
                .IncludeBase<Bookstore.Domains.People.Models.Subject, Models.Subject>()
                .ForMember(c => c.Locations, opt => opt.Ignore());
            // odd, ReverseMap() doesn't work here, so we have to make two declarations
            CreateMap<Models.Company, Bookstore.Domains.People.Models.Company>()
                .IncludeBase<Models.Subject, Bookstore.Domains.People.Models.Subject>();
        }
    }
}
