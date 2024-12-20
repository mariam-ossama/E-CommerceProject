using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


// Needs to run the migration and test
namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShoppingCartController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("add-to-basket")]
        public async Task<IActionResult> AddToBasket(string userId, int productId, int quantity)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (product.ProductQuantity < quantity)
            {
                return BadRequest("Not enough stock.");
            }

            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ProductId == productId);

            if (basketItem != null)
            {
                // Update quantity if item already exists in the basket
                basketItem.Quantity += quantity;
                if (basketItem.Quantity > product.ProductQuantity)
                {
                    return BadRequest("Not enough stock.");
                }

                _context.BasketItems.Update(basketItem);
            }
            else
            {
                // Add new item to the basket
                basketItem = new BasketItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };

                _context.BasketItems.Add(basketItem);
            }

            await _context.SaveChangesAsync();

            return Ok("Product added to basket.");
        }

        [HttpPatch("update-basket-item/{basketItemId}")]
        public async Task<IActionResult> UpdateBasketItem(int basketItemId, int quantity)
        {
            var basketItem = await _context.BasketItems.FindAsync(basketItemId);
            if (basketItem == null)
            {
                return NotFound("Basket item not found.");
            }

            var product = await _context.Products.FindAsync(basketItem.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (quantity > product.ProductQuantity)
            {
                return BadRequest("Not enough stock.");
            }

            if (quantity == 0)
            {
                _context.BasketItems.Remove(basketItem);
            }
            else
            {
                basketItem.Quantity = quantity;
                _context.BasketItems.Update(basketItem);
            }

            await _context.SaveChangesAsync();

            return Ok("Basket item updated.");
        }
        [HttpGet("basket-items/{userId}")]
        public async Task<IActionResult> GetBasketItems(string userId)
        {
            var basketItems = await _context.BasketItems
                .Where(b => b.UserId == userId)
                .Include(b => b.Product)
                .ToListAsync();

            return Ok(basketItems);
        }
        [HttpDelete("remove-from-basket/{basketItemId}")]
        public async Task<IActionResult> RemoveFromBasket(int basketItemId)
        {
            var basketItem = await _context.BasketItems.FindAsync(basketItemId);
            if (basketItem == null)
            {
                return NotFound("Basket item not found.");
            }

            _context.BasketItems.Remove(basketItem);
            await _context.SaveChangesAsync();

            return Ok("Product removed from basket.");
        }


    }
}
