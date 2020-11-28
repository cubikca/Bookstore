using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Bookstore.Entities.People.Models;

namespace Bookstore.Entities.People
{
    public class PeopleProfile : Profile
    {
        public PeopleProfile()
        {
            CreateMap<Province, Domains.People.Models.Province>().ReverseMap();
            CreateMap<Country, Domains.People.Models.Country>().ReverseMap();
            CreateMap<EmailAddress, Domains.People.Models.EmailAddress>().ReverseMap();
            CreateMap<PhoneNumber, Domains.People.Models.PhoneNumber>().ReverseMap();
            CreateMap<Address, Domains.People.Models.Address>().ReverseMap();
            CreateMap<OnlinePresence, Domains.People.Models.OnlinePresence>().ReverseMap();
        }
    }
}
