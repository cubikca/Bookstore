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
            ob.UseSqlServer("Data Source=sqlserver;Initial Catalog=PeopleDevelopment;Integrated Security=True");
            return new PeopleContext(ob.Options);
        }
    }
}
