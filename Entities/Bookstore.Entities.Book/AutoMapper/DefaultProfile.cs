using AutoMapper;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<Domains.Book.Models.Book, Models.Book>()
                .ForMember(b => b.Authors, opt => opt.Ignore())
                .ForMember(b => b.Publisher, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Domains.Book.Models.Author, Models.Author>()
                .ForMember(a => a.ProfileId, opt => opt.MapFrom((a => a.Profile.Id)))
                .ReverseMap()
                .ForMember(a => a.Profile, opt => opt.Ignore());
            CreateMap<Domains.Book.Models.Publisher, Models.Publisher>()
                .ForMember(p => p.ProfileId, opt => opt.MapFrom(p => p.Profile.Id))
                .ForMember(p => p.Books, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(p => p.Profile, opt => opt.Ignore());
        }
    }
}