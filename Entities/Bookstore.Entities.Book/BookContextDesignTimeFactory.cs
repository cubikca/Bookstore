using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookstore.Entities.Book
{
    public class BookContextDesignTimeFactory : IDesignTimeDbContextFactory<BookContext>
    {
        public BookContext CreateDbContext(string[] args)
        {
            var ob = new DbContextOptionsBuilder<BookContext>();
            var connectionString = "Data Source=sqlserver;User Id=brian;Password=development;Initial Catalog=BooksDevelopment";
            ob.UseSqlServer(connectionString);
            return new BookContext(ob.Options);
        }
    }
}