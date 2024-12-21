using E_Commerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//TODO: Testing and running the migrations
namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetOrders([FromQuery] string status)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var ordersQuery = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var orders = await ordersQuery.ToListAsync();

            var result = orders.Select(o => new
            {
                o.OrderId,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                Items = o.OrderItems.Select(oi => new
                {
                    oi.Product.ProductName,
                    oi.Quantity,
                    oi.Product.ProductPrice
                })
            });

            return Ok(result);
        }
    }
}
