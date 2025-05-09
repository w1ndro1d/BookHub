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
    public class OrderController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public OrderController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int GetMemberId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var memberId = GetMemberId();

            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Cart is empty");

            var order = new Order
            {
                MemberId = memberId,
                OrderDate = DateTime.Now,
                ClaimCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                OrderItems = new List<OrderItem>(),
                TotalAmount = 0
            };

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Book.Price
                };
                order.TotalAmount += orderItem.Quantity * orderItem.UnitPrice;
                order.OrderItems.Add(orderItem);
            }

            _dbContext.Orders.Add(order);
            _dbContext.CartItems.RemoveRange(cart.Items); // Clear cart
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                order.Id,
                order.ClaimCode,
                order.TotalAmount,
                order.OrderDate
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var memberId = GetMemberId();

            var orders = await _dbContext.Orders
                .Where(o => o.MemberId == memberId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .ToListAsync();

            return Ok(orders.Select(o => new
            {
                o.Id,
                o.ClaimCode,
                o.TotalAmount,
                o.OrderDate,
                Items = o.OrderItems.Select(i => new
                {
                    i.Book.Title,
                    i.Quantity,
                    i.UnitPrice
                })
            }));
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> RemoveOrder(int orderId)
        {
            var memberId = GetMemberId();

            // find the order by its ID and ensure it belongs to the currently logged-in user
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MemberId == memberId);

            if (order == null)
                return NotFound("Order not found or you do not have permission to delete this order.");

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Order successfully removed." });
        }
    }
}
