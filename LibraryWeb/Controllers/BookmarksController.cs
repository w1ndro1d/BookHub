using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace LibraryWeb.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BookmarksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "You must be logged in to view bookmarks.";
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var bookmarks = await client.GetFromJsonAsync<List<Book>>("api/bookmark");

            return View(bookmarks);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBookmark(int bookId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/bookmark/{bookId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Bookmark removed!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["NotificationMessage"] = $"Failed to remove from bookmark list! {error}";
            }

            return RedirectToAction("Index");
        }
    }
}
