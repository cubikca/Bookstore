using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace Bookstore.Entities.Book.AutoMapper
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<Domains.Book.Models.Book, Models.Book>()
                .ForMember(b => b.Authors, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
