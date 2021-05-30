using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookstore.Entities.People
{
    public class PeopleContextDesignTimeFactory : IDesignTimeDbContextFactory<PeopleContext>
    {
        public PeopleContext CreateDbContext(string[] args)
        {
            var ob = new DbContextOptionsBuilder<PeopleContext>();
            var connectionString = "server=mysql;user=brian;password=development;database=PeopleDevelopment";
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            ob.UseMySql(connectionString, serverVersion);
            return new PeopleContext(ob.Options);
        }
    }
}
