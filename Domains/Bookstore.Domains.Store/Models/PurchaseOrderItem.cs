namespace Bookstore.Domains.Store.Models
{
    public class PurchaseOrderItem
    {
        public Book.Models.Book Book { get; set; }
        public int OrderQuantity { get; set; }
        public int ReceiveQuantity { get; set; }
    }
}
