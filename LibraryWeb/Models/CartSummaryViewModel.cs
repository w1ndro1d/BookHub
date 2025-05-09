namespace LibraryWeb.Models
{
    public class CartSummaryViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public List<string> AppliedDiscount { get; set; } = new();
    }
}
