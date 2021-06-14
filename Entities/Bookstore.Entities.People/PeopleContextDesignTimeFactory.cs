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
            var connectionString = "Data Source=sqlserver;User Id=brian;Password=development;Initial Catalog=PeopleDevelopment";
            ob.UseSqlServer(connectionString);
            return new PeopleContext(ob.Options);
        }
    }
}
