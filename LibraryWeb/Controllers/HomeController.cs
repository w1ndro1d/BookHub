using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LibraryWeb.Controllers
{
    public class HomeController : Controller
    {
       public IActionResult Index()
        {
            return View();
        }

        public IActionResult BrowseBooks()
        {
            return RedirectToAction("Index", "Books");
        }
    }
}
