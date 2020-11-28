using System;
using System.Collections.Generic;
using System.Linq;
using Bookstore.Domains.People.Models;
using Bookstore.Domains.Book.Models;

namespace Bookstore.Domains.Store.Models
{
    public class Store
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
        public List<Shelf> Shelves { get; set; }
        public List<Basket> Baskets { get; set; }
        public Stock Stock { get; set; }
        public List<PurchaseOrder> PurchaseOrders { get; set; }
        public List<Sale> Sales { get; set; }

        public int ActualInventory(Book.Models.Book book)
        {
            var stock = Stock.Items.Where(i => i.Book.ISBN == book.ISBN).Sum(i => i.Quantity);
            var baskets = Baskets.SelectMany(b => b.Items).Where(i => i.Book.ISBN == book.ISBN).Sum(i => i.Quantity);
            var shelves = Shelves.SelectMany(s => s.Items).Where(i => i.Book.ISBN == book.ISBN).Sum(i => i.Quantity);
            return stock + baskets + shelves;
        }

        public int BookInventory(Book.Models.Book book)
        {
            var received = PurchaseOrders.SelectMany(o => o.Items).Where(i => i.Book.ISBN == book.ISBN).Sum(i => i.ReceiveQuantity);
            var sold = Sales.SelectMany(s => s.Items).Where(i => i.Book.ISBN == book.ISBN).Sum(i => i.Quantity);
            return received - sold;
        }
    }
}
