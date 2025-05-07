namespace LibraryWeb.Models
{
    public class OrderConfirmationViewModel
    {
        public int Id { get; set; }
        public string ClaimCode { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
