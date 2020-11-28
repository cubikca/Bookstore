using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Entities.Book
{
    public class BookContext : DbContext
    {
        public DbSet<Models.Book> Books { get; set; }
    }
}
