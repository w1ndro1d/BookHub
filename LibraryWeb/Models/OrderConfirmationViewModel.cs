namespace LibraryWeb.Models
{
    public class OrderConfirmationViewModel
    {
        public int Id { get; set; }
        public string ClaimCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool HasDiscount => DiscountAmount > 0;
        public DateTime OrderDate { get; set; }
        public List<string> AppliedDiscount { get; set; }
        public decimal SubTotalBeforeDiscount => TotalAmount + DiscountAmount;
    }
}
