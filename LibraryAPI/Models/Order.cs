namespace LibraryAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public bool IsCancelled { get; set; } = false;
        public bool IsClaimed { get; set; } = false;
        public required string ClaimCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public required ICollection<OrderItem> OrderItems { get; set; }
    }
}
