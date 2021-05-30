using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface IProvinceRepository
    {
        Task<Province> SaveProvince(Province province);
        Task<Province> FindProvinceByAbbreviation(string abbreviation);
        // we really don't want to carry around an array of provinces everywhere we use a country
        // thus, Provinces is not a property of Country and we provide a finder instead
        Task<ICollection<Province>> FindProvincesByCountryAbbreviation(string countryAbbreviation);
        Task<ICollection<Province>> FindAllProvinces();
        Task<bool> RemoveProvince(string abbreviation);
    }
}