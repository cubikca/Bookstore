using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Entities.Store.Models
{
    public class Store
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LocationId { get; set; }
        public IList<Shelf> Shelves { get; set; }
    }
}
