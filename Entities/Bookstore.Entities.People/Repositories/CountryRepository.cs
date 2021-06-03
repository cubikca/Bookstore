using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Bookstore.Domains.People;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.People.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Entities.People.Repositories
{
    public class CountryRepository : RepositoryBase<Country, Models.Country>, ICountryRepository
    {
        public CountryRepository(IDbContextFactory<PeopleContext> dbFactory, IMapper mapper, ILogger<RepositoryBase<Country, Models.Country>> logger) : base(dbFactory, mapper, logger)
        {
        }

        public override async Task<bool> Remove(Guid countryId)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled);
                await using var db = DbFactory.CreateDbContext();
                var entity = await db.Countries.FindAsync(countryId);
                if (entity == null) return false;
                foreach (var province in db.Provinces.Where(p => p.CountryId == countryId))
                    province.Deleted = true;
                entity.Deleted = true;
                await db.SaveChangesAsync();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                var msg = "Unable to remove Entity of type Country";
                Logger.LogError(ex, msg);
                throw new PeopleException(msg, ex);
            }
        }
    }
}