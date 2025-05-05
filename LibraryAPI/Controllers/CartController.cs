using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    public class CartController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public CartController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var member = await _dbContext.Members
            .Include(m => m.Cart)
                .FirstOrDefaultAsync(m => m.Email == User.Identity.Name);

            var book = await _dbContext.Books.FindAsync(bookId);
            if (member != null && book != null)
            {
                var cartItem = new CartItem
                {
                    BookId = bookId,
                    Quantity = 1
                };

                _dbContext.CartItems.Add(cartItem);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Books");
        }
    }
}
