using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface IProvinceRepository : IRepository<Province>
    {
        Task<ICollection<Province>> FindByCountry(Guid countryId);
    }
}