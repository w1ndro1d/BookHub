using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace LibraryWeb.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "You must be logged in to view your cart.";
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var cartItems = await client.GetFromJsonAsync<List<CartItemViewModel>>("api/cart");

            return View(cartItems ?? new List<CartItemViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("api/order", null);

            if (response.IsSuccessStatusCode)
            {
                var orderResult = await response.Content.ReadFromJsonAsync<OrderConfirmationViewModel>();
                return View("Confirmation", orderResult);
            }

            TempData["Error"] = "Failed to place order.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["BookmarkMessage"] = "Please log in to add items to your cart.";
                return RedirectToAction("Index", "Books");
            }

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"api/cart/add/{bookId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["BookmarkMessage"] = "Book added to cart successfully! 🛒";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["BookmarkMessage"] = $"Failed to add to cart: {error}";
            }

            //redirect to calling page and display toast message inside each page
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await client.DeleteAsync($"api/cart/remove/{bookId}");

            return RedirectToAction("Index");
        }
    }
}
