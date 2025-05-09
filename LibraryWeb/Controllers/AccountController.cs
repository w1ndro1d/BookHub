using LibraryWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.PostAsJsonAsync("api/Authentication/register", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["NotificationMessage"] = "Successfully registered!";
                return RedirectToAction("Login");
            }

            TempData["NotificationMessage"] = "Registration failed! If you're already registered, please use the login page.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.PostAsJsonAsync("api/Authentication/login", model);
            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResult>();
                HttpContext.Session.SetString("JWTToken", result.Token);

                //also set member id to see who is logged in
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(result.Token);
                var claims = jwt.Claims;

                //create ClaimsIdentity from JWT claims
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // sign the user in with cookie auth
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                var memberId = claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
                if (memberId != null)
                {
                    HttpContext.Session.SetInt32("MemberId", int.Parse(memberId));
                    TempData["NotificationMessage"] = "Successfully logged in! Redirecting to listings page...";
                }

                return RedirectToAction("Index", "Books");
            }

            TempData["NotificationMessage"] = "Login failed! Please try again.";
            return View(model);
        }

        public class LoginResult
        {
            public string Token { get; set; }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWTToken");
            // Sign out from cookie auth
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
