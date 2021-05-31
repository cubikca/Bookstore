using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface IAddressRepository
    {
        Task<Address> SaveAddress(Address address);
        Task<ICollection<Address>> FindAllAddresses();
        Task<Address> FindAddressById(Guid addressId);
        Task<bool> RemoveAddress(Guid addressId);
    }
}