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
                return Ok(new { Items = new List<object>(), Subtotal = 0, Discount = 0, Total = 0 });

            //var result = member.Cart.Items.Select(ci => new
            //{
            //    ci.BookId,
            //    ci.Book.Title,
            //    ci.Book.Author,
            //    ci.Book.Price,
            //    ci.Quantity
            //});

            var summary = CalculateCartSummary(member.Cart, member.Id);
            return Ok(summary);
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

        //for discount on 5 or more orders at once(5 or more orders at a time in cart)
        private object CalculateCartSummary(Cart cart, int memberId)
        {
            var items = cart.Items.Select(ci => new
            {
                ci.BookId,
                ci.Book.Title,
                ci.Book.Author,
                ci.Book.Price,
                ci.Quantity,
                ci.Book.Discount,   //listed discount in db
                TotalPrice = (ci.Book.Price * ci.Quantity) - ci.Book.Discount * (ci.Book.Price * ci.Quantity)   //we need to subtract individual discount amount if its applicable as well
            }).ToList();

            var subtotal = items.Sum(i => i.TotalPrice);
            var totalQuantity = items.Sum(i => i.Quantity);
            
            decimal discountAmount = 0;
            var discounts = new List<string>();
            
            //loyalty Discount (every 10th order)
            var orderCount = _dbContext.Orders.Count(o => o.MemberId == memberId) + 1;
            if (orderCount > 0 && orderCount % 10 == 0)
            {
                discountAmount += subtotal * 0.10m;
                discounts.Add("10% Loyalty Discount (only available after 10 successful orders)");
            }

            //bulk purchase discount(5%)
            if (totalQuantity >= 5)
            {
                discountAmount += subtotal * 0.05m;
                discounts.Add("5% Bulk Purchase Discount");
            }

            //total here is already subtracting discount amount, so if we are to display this in our razor page, we should add discount amount again to get total
            var total = subtotal - discountAmount;

            return new
            {
                Items = items,
                Subtotal = subtotal,
                Discount = discountAmount,
                Total = total,
                AppliedDiscount = discounts
            };
        }
    }
}
