namespace LibraryWeb.Models
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
