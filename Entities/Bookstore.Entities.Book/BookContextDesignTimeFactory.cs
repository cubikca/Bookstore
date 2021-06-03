using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookstore.Entities.Book
{
    public class BookContextDesignTimeFactory : IDesignTimeDbContextFactory<BookContext>
    {
        public BookContext CreateDbContext(string[] args)
        {
            var ob = new DbContextOptionsBuilder<BookContext>();
            var connectionString = "server=mysql;user=brian;password=development;database=BooksDevelopment";
            ob.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            return new BookContext(ob.Options);
        }
    }
}