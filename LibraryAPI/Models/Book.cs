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
        public int InStock { get; set; }  //0-out of stock, 1-stock available for purchase, 2-available only in library(can't be purchased)
        public required string Language { get; set; }
        public required string Format { get; set; }
        public required string ISBN { get; set; }
        public double Rating { get; set; }
        public DateTime PublicationDate { get; set; }

        //added later
        public bool IsBestseller { get; set; }
        public bool HasAwards { get; set; }
        public DateTime ListedDate { get; set; }
        public decimal Discount { get; set; }  // 0-no discount

    }
}
