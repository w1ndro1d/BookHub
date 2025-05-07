using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace LibraryWeb.Controllers
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private int? GetLoggedInMemberId()
        {
            return HttpContext.Session.GetInt32("MemberId");
        }

        //apply pagination to display list of all books
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 15)
        {
            var client = _httpClientFactory.CreateClient("API");
            var books = await client.GetFromJsonAsync<List<Book>>("api/books/all");

            if(books == null)
            {
                return View(new List<Book>());
            }
            
            int totalBooks = books.Count;
            var pagedBooks = books.OrderBy(b => b.Title).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);
            ViewBag.CurrentPage = page;

            var viewModel = new BooksViewModel
            {
                Books = pagedBooks
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 15, string searchQuery = "", string filterAuthor = "", string filterGenre = "", decimal? filterPriceMin = null, decimal? filterPriceMax = null, string sortOrder = "", int? filterAvailability = null, string category = "")
        {
            var client = _httpClientFactory.CreateClient("API");
            var url = "api/Books?";
            bool isFirstParam = true;

            if (page > 0) // Ensure page is valid
            {
                url += $"{(isFirstParam ? "" : "&")}page={page}";
                isFirstParam = false;
            }

            if (pageSize > 0) // Ensure pageSize is valid
            {
                url += $"{(isFirstParam ? "" : "&")}pageSize={pageSize}";
                isFirstParam = false;
            }

            // Add filter parameters dynamically
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

            if (filterAvailability.HasValue)
            {
                url += $"{(isFirstParam ? "" : "&")}filterAvailability={filterAvailability}";
                isFirstParam = false;
            }

            if (!string.IsNullOrEmpty(category))
            {
                url += $"{(isFirstParam ? "" : "&")}category={Uri.EscapeDataString(category)}";
                isFirstParam = false;
            }

            // URL should have all parameters including pagination
            var books = await client.GetFromJsonAsync<List<Book>>(url);
            var pagedBooks = books.OrderBy(b => sortOrder).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(books.Count / (double)pageSize);
            ViewBag.CurrentPage = page;

            var viewModel = new BooksViewModel
            {
                Books = pagedBooks,
                SearchQuery = searchQuery,
                FilterAuthor = filterAuthor,
                FilterGenre = filterGenre,
                FilterPriceMin = filterPriceMin,
                FilterPriceMax = filterPriceMax,
                SortOrder = sortOrder,
                FilterAvailability = filterAvailability,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(books.Count / (double)pageSize),
                Category = category
            }; // Return filtered/sorted and paginated books

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Bookmark(int bookId)
        {
            var token = HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["BookmarkMessage"] = "You must be logged in to bookmark books.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { BookId = bookId };
            var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/bookmark", json);

            if (response.IsSuccessStatusCode)
            {
                TempData["BookmarkMessage"] = "Bookmark successful!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["BookmarkMessage"] = $"Failed to bookmark: {error}";
            }

            return RedirectToAction("Index");
        }

    }
}
