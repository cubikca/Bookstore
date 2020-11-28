using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.People.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company> SaveCompany(Company company);
        Task<IList<Company>> FindAllCompanies();
        Task<Company> FindCompanyById(Guid companyId);
        Task<bool> RemoveCompany(Guid companyId);
    }
}
