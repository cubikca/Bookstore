using System;
using System.Collections.Generic;

namespace Bookstore.Domains.Store.Models
{
    public class Basket
    {
        public Guid ShopperId { get; set; }
        public List<SaleItem> Items { get; set; }
        public DateTimeOffset ExpireTime { get; set; }
    }
}
