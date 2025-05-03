using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("API");
            var books = await client.GetFromJsonAsync<List<Book>>("api/books/all");
            return View(books);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery, string filterAuthor, string filterGenre, decimal? filterPriceMin, decimal? filterPriceMax, string sortOrder)
        {
            var client = _httpClientFactory.CreateClient("API");

            var url = "api/Books?"; // Start the URL correctly without the extra &

            // Add filter parameters dynamically, starting from the first filter
            bool isFirstParam = true;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                url += $"{(isFirstParam ? "" : "&")}searchQuery={Uri.EscapeDataString(searchQuery)}";
                isFirstParam = false;
            }

            if (!string.IsNullOrEmpty(filterAuthor))
            {
                url += $"{(isFirstParam ? "" : "&")}filterAuthor={Uri.EscapeDataString(filterAuthor)}";
                isFirstParam = false;
            }

            if (!string.IsNullOrEmpty(filterGenre))
            {
                url += $"{(isFirstParam ? "" : "&")}filterGenre={Uri.EscapeDataString(filterGenre)}";
                isFirstParam = false;
            }

            if (filterPriceMin.HasValue)
            {
                url += $"{(isFirstParam ? "" : "&")}filterPriceMin={filterPriceMin}";
                isFirstParam = false;
            }

            if (filterPriceMax.HasValue)
            {
                url += $"{(isFirstParam ? "" : "&")}filterPriceMax={filterPriceMax}";
                isFirstParam = false;
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                url += $"{(isFirstParam ? "" : "&")}sortOrder={Uri.EscapeDataString(sortOrder)}";
            }

            // Now, the URL should be well-formed
            var books = await client.GetFromJsonAsync<List<Book>>(url);
            return View(books); // Return filtered/sorted books
        }
    }
}
