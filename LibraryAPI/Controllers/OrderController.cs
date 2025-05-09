using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;

        public OrderController(LibraryDbContext dbContext, IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
        }

        private int GetMemberId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var memberId = GetMemberId();
            var member = await _dbContext.Members.FirstOrDefaultAsync(m => m.Id == memberId);

            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Cart is empty");

            int totalBooks = cart.Items.Sum(i => i.Quantity);
            decimal subTotal = 0;

            var order = new Order
            {
                MemberId = memberId,
                OrderDate = DateTime.Now,
                ClaimCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Book.Price
                };

                order.OrderItems.Add(orderItem);
            }

            // Compute totals & discounts
            var appliedDiscounts = await ApplyDiscountsAndCalculateTotals(order, memberId);

            _dbContext.Orders.Add(order);
            _dbContext.CartItems.RemoveRange(cart.Items); // clear cart
            await _dbContext.SaveChangesAsync();

            //send out confirmation email
            if (member != null && !string.IsNullOrEmpty(member.Email))
            {
                try
                {
                    await SendConfirmationEmail(order, member.Email, appliedDiscounts);
                }
                catch (Exception ex)
                {
                    return NotFound($"Failed to send email to {member.Email}.");
                }
            }
            
            return Ok(new
            {
                order.Id,
                order.ClaimCode,
                order.TotalAmount,
                order.DiscountAmount,
                appliedDiscount = appliedDiscounts,
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

            // find the order by its ID and check if it belongs to the currently logged-in user
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MemberId == memberId);

            if (order == null)
                return NotFound("Order not found or you do not have permission to delete this order.");

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
            
            return Ok(new { message = "Order successfully removed." });
        }


        // refactored discount and total logic
        private async Task<List<string>> ApplyDiscountsAndCalculateTotals(Order order, int memberId)
        {
            var subTotal = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
            int totalBooks = order.OrderItems.Sum(i => i.Quantity);
            var discounts = new List<string>();
            decimal discountAmount = 0;

            // Loyalty Discount (every 10th order)
            // +1 because the orders are only counted before they are saved
            var orderCount = await _dbContext.Orders.CountAsync(o => o.MemberId == memberId) + 1;
            if (orderCount > 0 && orderCount % 10 == 0)
            {
                discountAmount += subTotal * 0.10m;
                discounts.Add("10% Loyalty Discount (only available after 10 successful orders)");
            }

            // Bulk purchase discount
            if (totalBooks >= 5)
            {
                discountAmount += subTotal * 0.05m;
                discounts.Add("5% Bulk Purchase Discount");
            }

            order.DiscountAmount = discountAmount;
            order.TotalAmount = subTotal - discountAmount;
            return discounts;
        }

        private async Task SendConfirmationEmail(Order order, string recipientEmail, List<string> appliedDiscounts)
        {
            decimal subTotal = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
            
            var emailBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #ddd;border-radius:6px;overflow:hidden;'>
                <div style='background:#f8f9fa;padding:16px;font-size:16px;border-bottom:1px solid #ccc;'>
                    <strong>Order #{order.Id}</strong> |
                    Claim Code: <span style='color:green;'>{order.ClaimCode}</span> |
                    Total: <strong>$ {order.TotalAmount:N2}</strong> |
                    Date: {order.OrderDate:yyyy-MM-dd HH:mm}
                </div>
                <div style='padding:16px;'>
                    <table style='width:100%;border-collapse:collapse;font-size:14px;'>
                        <thead>
                            <tr>
                                <th style='border-bottom:1px solid #ddd;text-align:left;padding:8px;'>Book</th>
                                <th style='border-bottom:1px solid #ddd;text-align:left;padding:8px;'>Quantity</th>
                                <th style='border-bottom:1px solid #ddd;text-align:left;padding:8px;'>Unit Price</th>
                                <th style='border-bottom:1px solid #ddd;text-align:left;padding:8px;'>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>";

            foreach (var item in order.OrderItems)
            {
                emailBody += $@"
                            <tr>
                                <td style='padding:8px;border-bottom:1px solid #eee;'>{item.Book.Title}</td>
                                <td style='padding:8px;border-bottom:1px solid #eee;'>{item.Quantity}</td>
                                <td style='padding:8px;border-bottom:1px solid #eee;'>$ {item.UnitPrice:N2}</td>
                                <td style='padding:8px;border-bottom:1px solid #eee;'>$ {item.Quantity * item.UnitPrice:N2}</td>
                            </tr>";
            }

                emailBody += $@"
                            </tbody>
                        </table>

                        <p style='margin-top:16px;'>
                            <strong>Total:</strong> $ {subTotal:N2}<br/>
                            <strong>Discount Amount:</strong> -$ {order.DiscountAmount:N2}<br/>
                            <strong>Total Payable Amount:</strong> $ {order.TotalAmount:N2}
                        </p>";

            if (appliedDiscounts.Any())
            {
                emailBody += @"
                    <p style='margin-top:8px;'>
                        <strong>Discounts Applied:</strong><br/>
                        <ul style='padding-left:20px;margin-top:4px;margin-bottom:12px;'>";

                foreach (var discount in appliedDiscounts)
                {
                    emailBody += $"<li>{discount}</li>";
                }

                emailBody += @"</ul>
                    </p>";
            }

            emailBody += @"
                    <p style='margin-top:20px;'>Please bring your <strong>Membership ID</strong> and <strong>Claim Code</strong> to the store when picking up your order.</p>
                    <p><em>You can print or take a screenshot of this email for easy reference during pickup.</em></p>
                </div>
                <div style='background:#f1f1f1;padding:12px;font-size:12px;color:#666;text-align:center;'>
                    This is an automated message. For questions, contact support at +977 9841989988.
                </div>
            </div>";

            await _emailSender.SendEmailAsync(
                recipientEmail,
                "Order Confirmation - Your Claim Code and Bill",
                emailBody
            );
        }
    }
}
