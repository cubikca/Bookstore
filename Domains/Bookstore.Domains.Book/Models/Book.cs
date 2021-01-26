using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Book.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public IList<Author> Authors { get; set; }
        public int Edition { get; set; }
        public DateTime PublishDate { get; set; }
        public Subject Publisher { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
    }
}
