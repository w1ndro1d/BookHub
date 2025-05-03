namespace LibraryAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public required Member Member { get; set; }
        public int BookId { get; set; }
        public required Book Book { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public required string Comment { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    }
}
