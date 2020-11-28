using AutoMapper;

namespace Bookstore.Entities.People.AutoMapper
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<Domains.People.Models.Address, Models.Address>().ReverseMap();
            CreateMap<Domains.People.Models.EmailAddress, Models.EmailAddress>().ReverseMap();
            CreateMap<Domains.People.Models.PhoneNumber, Models.PhoneNumber>().ReverseMap();
            CreateMap<Domains.People.Models.OnlinePresence, Models.OnlinePresence>() .ReverseMap();
            CreateMap<Domains.People.Models.Province, Models.Province>().ReverseMap();
            CreateMap<Domains.People.Models.Country, Models.Country>().ReverseMap();
        }
    }
}
