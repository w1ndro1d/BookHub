using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace LibraryWeb.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var orders = await client.GetFromJsonAsync<List<OrderViewModel>>("api/order");

            return View(orders ?? new List<OrderViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromOrders(int orderId)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/order/{orderId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Order cancelled!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["NotificationMessage"] = $"Failed to cancel order: {error}";
            }

            return RedirectToAction("Index");
        }
    }
}
