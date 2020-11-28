using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Store.Models
{
    public class Sale
    {
        public Subject SoldTo { get; set; }
        public Address BillingAddress { get; set; }
        public List<SaleItem> Items { get; set; }
    }
}
