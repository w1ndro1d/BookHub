using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LibraryWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.GetAsync("api/booksadmin/announcements/public");
            var announcements = new List<Announcement>();

            if (response.IsSuccessStatusCode)
            {
                announcements = await response.Content.ReadFromJsonAsync<List<Announcement>>();
            }

            var today = DateTime.Today;
            var pinnedAnnouncement = announcements
                                    .FirstOrDefault(a =>
                                        a.IsPinned == true &&
                                        a.StartDate.HasValue && a.EndDate.HasValue &&
                                        a.StartDate.Value.Date <= today &&
                                        a.EndDate.Value.Date >= today
                                    );
            ViewBag.PinnedAnnouncement = pinnedAnnouncement;

            //// Clear session on app start to ensure user is logged out
            //Response.Cookies.Delete(".AspNetCore.Cookies");
            //HttpContext.Session.Clear();

            return View(announcements);
        }

        public IActionResult BrowseBooks()
        {
            return RedirectToAction("Index", "Books");
        }
    }
}
