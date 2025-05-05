namespace LibraryWeb.Models
{
    public class BooksViewModel
    {
        public List<Book> Books { get; set; } = new();
        public string SearchQuery { get; set; }
        public string FilterAuthor { get; set; }
        public string FilterGenre { get; set; }
        public decimal? FilterPriceMin { get; set; }
        public decimal? FilterPriceMax { get; set; }
        public string SortOrder { get; set; }
        public int? FilterAvailability { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages { get; set; }

        //for type of book
        public string Category { get; set; } = "All";   //default category should be All
    }
}
