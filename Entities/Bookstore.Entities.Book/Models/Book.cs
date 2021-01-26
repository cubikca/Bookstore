using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bookstore.Domains.Book.Models;
using Bookstore.Domains.People.Models;

namespace Bookstore.Entities.Book.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public virtual IList<Author> Authors { get; set; }
        public int Edition { get; set; }
        public DateTime PublishDate { get; set; }
        public virtual Publisher Publisher { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
   }
}
