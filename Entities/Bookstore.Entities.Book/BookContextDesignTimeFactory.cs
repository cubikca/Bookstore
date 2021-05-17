using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookstore.Entities.Book
{
    public class BookContextDesignTimeFactory : IDesignTimeDbContextFactory<BookContext>
    {
        public BookContext CreateDbContext(string[] args)
        {
            var ob = new DbContextOptionsBuilder<BookContext>();
            ob.UseSqlServer("Data Source=sqlserver;Initial Catalog=BookDevelopment;User Id=brian;Password=development");
            return new BookContext(ob.Options);
        }
    }
}