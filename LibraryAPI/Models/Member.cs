namespace LibraryAPI.Models
{
    public class Member
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; } 
        public required string MembershipId { get; set; }
        public int SuccessfulOrdersCount { get; set; } = 0; // For 10% discount
        public virtual Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Book> Whitelist { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
