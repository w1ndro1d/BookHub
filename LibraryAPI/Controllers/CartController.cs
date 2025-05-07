using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public CartController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private async Task<Member?> GetLoggedInMemberAsync()
        {
            var memberId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            return await _dbContext.Members
                .Include(m => m.Cart)
                    .ThenInclude(c => c.Items)
                        .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(m => m.Id == memberId);
        }


        [HttpPost("add/{bookId}")]
        [Authorize]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var member = await GetLoggedInMemberAsync();
            var book = await _dbContext.Books.FindAsync(bookId);

            if (member == null || book == null)
                return BadRequest("Invalid member or book");

            // Create a cart if it doesn't exist
            if (member.Cart == null)
            {
                member.Cart = new Cart
                {
                    MemberId = member.Id,
                    Items = new List<CartItem>()
                };
                _dbContext.Carts.Add(member.Cart);
            }

            // Check if item already in cart
            var existingItem = member.Cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                member.Cart.Items.Add(new CartItem
                {
                    BookId = bookId,
                    Quantity = 1
                });
            }

            await _dbContext.SaveChangesAsync();
            return Ok("Added to cart");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var member = await GetLoggedInMemberAsync();

            if (member?.Cart == null || !member.Cart.Items.Any())
                return Ok(new List<object>());

            var result = member.Cart.Items.Select(ci => new
            {
                ci.BookId,
                ci.Book.Title,
                ci.Book.Author,
                ci.Book.Price,
                ci.Quantity
            });

            return Ok(result);
        }

        [HttpDelete("remove/{bookId}")]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var member = await GetLoggedInMemberAsync();
            if (member?.Cart == null)
                return BadRequest("No cart found");

            var item = member.Cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
            if (item == null)
                return NotFound("Book not in cart");
            
            member.Cart.Items.Remove(item);
            await _dbContext.SaveChangesAsync();

            return Ok("Removed from cart");
        }
    }
}
