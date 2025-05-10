namespace LibraryWeb.Models
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }   //this is the original listed discount in db
    }
}
