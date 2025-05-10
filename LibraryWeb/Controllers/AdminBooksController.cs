using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminBooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAuthorizedClient()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAuthorizedClient();
            var books = await client.GetFromJsonAsync<List<Book>>("api/booksadmin");
            return View(books);
        }

        public IActionResult Create() => View();

        #region ADMIN CRUD
        [HttpPost]
        public async Task<IActionResult> Create(Book model)
        {
            //foreach (var state in ModelState)
            //{
            //    if (state.Value.Errors.Count > 0)
            //    {
            //        Console.WriteLine($"❌ Field '{state.Key}' has error: {state.Value.Errors[0].ErrorMessage}");
            //    }
            //}

            if (!ModelState.IsValid) return View(model);

            var client = GetAuthorizedClient();
            var response = await client.PostAsJsonAsync("api/booksadmin", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Book listing added!";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["NotificationMessage"] = $"Failed to add book! {error}";
            }

            //ModelState.AddModelError("", "Failed to create book.");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = GetAuthorizedClient();
            var book = await client.GetFromJsonAsync<List<Book>>($"api/booksadmin/");
            var target = book.FirstOrDefault(b => b.Id == id);

            if (target == null) return NotFound();
            return View(target);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Book model)
        {
            var client = GetAuthorizedClient();
            var response = await client.PutAsJsonAsync($"api/booksadmin/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Book listing edited!";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["NotificationMessage"] = $"Failed to edit book! {error}";
            }

            //ModelState.AddModelError("", "Failed to update book.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetAuthorizedClient();
            var response = await client.DeleteAsync($"api/booksadmin/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Book listing removed!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["NotificationMessage"] = $"Failed to remove book! {error}";
            }

            return RedirectToAction("Index");
        }

        #endregion ADMIN CRUD

        #region ADMIN ANNOUNCEMENTS

        public async Task<IActionResult> Announcements()
        {
            var client = GetAuthorizedClient();
            var response = await client.GetAsync("api/booksadmin/announcements");
            if (!response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Failed to load announcements!";
                return View(new List<Announcement>());
            }

            var data = await response.Content.ReadFromJsonAsync<List<Announcement>>();
            return View(data);
        }

        public IActionResult CreateAnnouncement()
        {
            return View(new Announcement());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement(Announcement model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = GetAuthorizedClient();
            var response = await client.PostAsJsonAsync("api/booksadmin/announcements", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Announcement created!";
                return RedirectToAction("Announcements");
            }

            TempData["NotificationMessage"] = "Failed to create announcement.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            var client = GetAuthorizedClient();
            var announcements = await client.GetFromJsonAsync<List<Announcement>>("api/booksadmin/announcements");

            if (announcements == null)
            {
                TempData["NotificationMessage"] = "Announcements could not be loaded.";
                return RedirectToAction("Announcements");
            }

            var model = announcements.FirstOrDefault(a => a.Id == id);
            if (model == null)
            {
                TempData["NotificationMessage"] = $"Announcement with ID {id} not found.";
                return RedirectToAction("Announcements");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(int id, Announcement model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = GetAuthorizedClient();
            var response = await client.PutAsJsonAsync($"api/booksadmin/announcements/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Announcement updated!";
                return RedirectToAction("Announcements");
            }

            TempData["NotificationMessage"] = "Failed to update announcement.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var client = GetAuthorizedClient();
            var response = await client.DeleteAsync($"api/booksadmin/announcements/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Announcement deleted.";
            }
            else
            {
                TempData["NotificationMessage"] = "Failed to delete announcement.";
            }

            return RedirectToAction("Announcements");
        }


        #endregion ADMIN ANNOUNCEMENTS
    }
}
