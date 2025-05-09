namespace LibraryWeb.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string ClaimCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<string> AppliedDiscount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }
}
