using AutoMapper;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class PublisherProfile : Profile
    {
        public PublisherProfile()
        {
            CreateMap<Domains.Book.Models.Publisher, Models.Publisher>()
                .ForMember(p => p.DetailsId, opt => opt.MapFrom(p => p.Details.Id))
                .ForMember(p => p.Books, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(p => p.Details, opt => opt.Ignore())
                .ForMember(p => p.Books, opt => opt.Ignore());
        }
    }
}