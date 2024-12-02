using E_Commerce.Data;
using E_Commerce.DTOs.ProductDTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


// TODO: Testing
namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct(CreateProductDTO dto)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            if (!await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
                return BadRequest("Invalid CategoryId.");

            var product = new Product
            {
                ProductName = dto.ProductName,
                CategoryId = dto.CategoryId,
                ProductPrice = dto.ProductPrice,
                ProductDescription = dto.ProductDescription,
                ProductQuantity = dto.ProductQuantity
            };

            // Map ProductImages from DTO
            foreach (var imagePath in dto.ProductImages)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImagePath = imagePath,
                    Product = product
                });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok("Product added successfully.");
        }


        [HttpPatch("edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] Product productUpdates)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Product not found.");

            if (!string.IsNullOrEmpty(productUpdates.ProductName))
                product.ProductName = productUpdates.ProductName;

            if (productUpdates.ProductPrice > 0)
                product.ProductPrice = productUpdates.ProductPrice;

            if (!string.IsNullOrEmpty(productUpdates.ProductDescription))
                product.ProductDescription = productUpdates.ProductDescription;

            product.ProductQuantity = productUpdates.ProductQuantity > 0
                ? productUpdates.ProductQuantity
                : product.ProductQuantity;

            await _context.SaveChangesAsync();
            return Ok("Product updated successfully.");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Product not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok("Product deleted successfully.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound("Product not found.");
            return Ok(product);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts(
    [FromQuery] string? categoryName = null,
    [FromQuery] string? sortBy = "ProductName",
    [FromQuery] string? sortOrder = "asc")
        {
            // Base query
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(p => p.Category.CategoryName == categoryName);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "productname" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.ProductName)
                    : query.OrderBy(p => p.ProductName),

                "price" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.ProductPrice)
                    : query.OrderBy(p => p.ProductPrice),

                "quantity" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.ProductQuantity)
                    : query.OrderBy(p => p.ProductQuantity),

                _ => query.OrderBy(p => p.ProductName) // Default sorting
            };

            // Execute query
            var products = await query.ToListAsync();
            return Ok(products);
        }

        [HttpDelete("{productId}/image")]
        public async Task<IActionResult> DeleteProductImage(int productId, [FromQuery] string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Image path must be provided.");
            }

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var imageToRemove = product.ProductImages.FirstOrDefault(img => img.ImagePath == imagePath);

            if (imageToRemove == null)
            {
                return NotFound("Image not found for this product.");
            }

            product.ProductImages.Remove(imageToRemove);
            _context.Images.Remove(imageToRemove); // Remove from the Image table
            await _context.SaveChangesAsync();

            var imageFullPath = Path.Combine("wwwroot", "images", "products", imagePath);
            if (System.IO.File.Exists(imageFullPath))
            {
                System.IO.File.Delete(imageFullPath); // Optional: delete the image from the file system
            }

            return Ok("Image removed successfully.");
        }

        [HttpPost("{productId}/add-image")]
        public async Task<IActionResult> AddImageToProduct(int productId, [FromBody] string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Image path must be provided.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var newImage = new ProductImage
            {
                ImagePath = imagePath,
                ProductId = productId
            };

            product.ProductImages.Add(newImage);
            await _context.SaveChangesAsync();

            return Ok("Image added successfully.");
        }

    }
}
