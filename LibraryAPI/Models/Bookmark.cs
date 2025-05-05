namespace LibraryAPI.Models
{
    public class Bookmark
    {
        public int Id { get; set; } 
        public int MemberId { get; set; }  // ID of the member who bookmarked the book
        public int BookId { get; set; }  // ID of the book being bookmarked
        public DateTime CreatedAt { get; set; }  // Timestamp of when the bookmark was added

        public virtual Member Member { get; set; } 
        public virtual Book Book { get; set; }
    }
}
