using AutoMapper;

namespace Bookstore.Entities.People.AutoMapper
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<Domains.People.Models.Address, Models.Address>()
                .ForMember(a => a.Province, opt => opt.Ignore())
                .ForMember(a => a.Country, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Domains.People.Models.EmailAddress, Models.EmailAddress>().ReverseMap();
            CreateMap<Domains.People.Models.PhoneNumber, Models.PhoneNumber>().ReverseMap();
            CreateMap<Domains.People.Models.OnlinePresence, Models.OnlinePresence>() .ReverseMap();
            CreateMap<Domains.People.Models.Province, Models.Province>().ReverseMap();
            CreateMap<Domains.People.Models.Country, Models.Country>().ReverseMap();
            CreateMap<Domains.People.Models.Person, Models.Person>()
                .ForMember(p => p.MailingAddress, opt => opt.Ignore())
                .ForMember(p => p.StreetAddress, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Domains.People.Models.Location, Models.Location>()
                .ForMember(l => l.StreetAddress, opt => opt.Ignore())
                .ForMember(l => l.MailingAddress, opt => opt.Ignore())
                .ForMember(l => l.Contacts, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
