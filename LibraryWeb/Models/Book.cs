namespace LibraryWeb.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int InStock { get; set; }    //0-out of stock, 1-stock available for purchase, 2-available only in library(can't be purchased)
        public string Language { get; set; }
        public string Format { get; set; }
        public string ISBN { get; set; }
        public double Rating { get; set; }
        public DateTime PublicationDate { get; set; }

        //added later
        public bool IsBestseller { get; set; }
        public bool HasAwards { get; set; }
        public DateTime ListedDate { get; set; } = DateTime.Now;
        public decimal Discount { get; set; }  // 0-no discount
    }
}
