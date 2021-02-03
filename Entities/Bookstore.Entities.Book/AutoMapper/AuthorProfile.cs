using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Domains.Book.Models.Author, Models.Author>()
                .ForMember(a => a.DetailsId, opt => opt.MapFrom((a => a.Details.Id)))
                .ReverseMap()
                .ForMember(a => a.Details, opt => opt.Ignore());
        }
    }
}
