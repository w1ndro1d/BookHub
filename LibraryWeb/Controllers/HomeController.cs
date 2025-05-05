using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LibraryWeb.Controllers
{
    public class HomeController : Controller
    {
       public IActionResult Index()
        {
            //// Clear session on app start to ensure user is logged out
            //Response.Cookies.Delete(".AspNetCore.Cookies");
            //HttpContext.Session.Clear();
            return View();
        }

        public IActionResult BrowseBooks()
        {
            return RedirectToAction("Index", "Books");
        }
    }
}
