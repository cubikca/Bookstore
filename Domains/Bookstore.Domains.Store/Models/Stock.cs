using System.Collections.Generic;

namespace Bookstore.Domains.Store.Models
{
    // This is what's "in the back"
    public class Stock
    {
        public List<SaleItem> Items { get; set; }
    }
}
