namespace Bookstore.Domains.Store.Models
{
    public class SaleItem
    {
        public Book.Models.Book Book { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
    }
}
