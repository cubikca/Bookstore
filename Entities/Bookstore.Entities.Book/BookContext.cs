using System;
using System.Collections.Generic;
using System.Text;
using Bookstore.Entities.Book.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Entities.Book
{
    public class BookContext : DbContext
    {
        public DbSet<Models.Book> Books { get; set; }
        public DbSet<Models.Author> Authors { get; set; }
        public DbSet<Models.Publisher> Publishers { get; set; }
    }
}
