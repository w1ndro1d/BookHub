namespace LibraryAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Genre { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public required string Language { get; set; }
        public required string Format { get; set; }
        public required string ISBN { get; set; }
        public int Rating { get; set; }
        public DateTime PublicationDate { get; set; }

    }
}
