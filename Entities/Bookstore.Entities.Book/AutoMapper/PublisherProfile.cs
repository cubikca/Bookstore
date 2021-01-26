using AutoMapper;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class PublisherProfile : Profile
    {
        public PublisherProfile()
        {
            CreateMap<Domains.Book.Models.Publisher, Models.Publisher>()
                .ReverseMap();
        }
    }
}