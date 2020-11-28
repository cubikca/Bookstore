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
            CreateMap<Domains.Book.Models.Author, Models.Author>().ReverseMap();
        }
    }
}
