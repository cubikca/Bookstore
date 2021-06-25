using AutoMapper;
using Bookstore.Domains.Book.Models;
using Bookstore.Entities.Book.Models;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<IDomainObject, IEntity>()
                .ForMember(b => b.Created, opt => opt.Ignore())
                .ForMember(b => b.CreatedBy, opt => opt.Ignore())
                .ForMember(b => b.Updated, opt => opt.Ignore())
                .ForMember(b => b.UpdatedBy, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Domains.Book.Models.Book, Models.Book>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(b => b.Authors, opt => opt.Ignore())
                .ForMember(b => b.Publisher, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Domains.Book.Models.Author, Models.Author>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(a => a.ProfileId, opt => opt.MapFrom((a => a.Profile.Id)))
                .ReverseMap()
                .ForMember(a => a.Profile, opt => opt.Ignore());
            CreateMap<Domains.Book.Models.Publisher, Models.Publisher>()
                .IncludeBase<IDomainObject, IEntity>()
                .ForMember(p => p.ProfileId, opt => opt.MapFrom(p => p.Profile.Id))
                .ForMember(p => p.Books, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(p => p.Profile, opt => opt.Ignore());
        }
    }
}