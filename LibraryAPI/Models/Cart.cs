namespace LibraryAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public virtual Member Member { get; set; }
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
