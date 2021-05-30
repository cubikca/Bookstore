using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface ILocationRepository
    {
        Task<Location> SaveLocation(Location location);
        Task<Location> FindLocationById(Guid locationId);
        Task<ICollection<Location>> FindAllLocations();
        Task<bool> RemoveLocation(Guid locationId);
    }
}