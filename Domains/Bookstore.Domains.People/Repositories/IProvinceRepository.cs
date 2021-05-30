using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface IProvinceRepository
    {
        Task<Province> SaveProvince(Province province);
        Task<Province> FindProvinceById(Guid provinceId);
        // we really don't want to carry around an array of provinces everywhere we use a country
        // thus, Provinces is not a property of Country and we provide a finder instead
        Task<ICollection<Province>> FindProvincesByCountryId(Guid countryId);
        Task<ICollection<Province>> FindAllProvinces();
        Task<bool> RemoveProvince(Guid provinceId);
    }
}