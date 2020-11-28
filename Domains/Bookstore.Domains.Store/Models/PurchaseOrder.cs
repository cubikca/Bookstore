using System;
using System.Collections.Generic;
using Bookstore.Domains.People.Models;

namespace Bookstore.Domains.Store.Models
{
    public class PurchaseOrder
    {
        public Subject Publisher { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public List<PurchaseOrderItem> Items { get; set; }
    }
}
