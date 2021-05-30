using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface ICountryRepository
    {
        Task<Country> SaveCountry(Country country);
        Task<Country> FindCountryById(Guid countryId);
        Task<ICollection<Country>> FindAllCountries();
        Task<bool> RemoveCountry(Guid countryId);
    }
}
