using System.Collections.Generic;

namespace Bookstore.Domains.Store.Models
{
    public class Shelf
    {
        public string Name { get; set; }
        public List<SaleItem> Items { get; set; }
    }
}
