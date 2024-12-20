using E_Commerce.Data;
using E_Commerce.DTOs.ProductDTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
        public async Task<IActionResult> EditProduct(int id, [FromBody] UpdateProductDTO productUpdates)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound("Product not found.");

            if (!string.IsNullOrEmpty(productUpdates.ProductName))
                product.ProductName = productUpdates.ProductName;

            if (productUpdates.ProductPrice.HasValue && productUpdates.ProductPrice.Value > 0)
                product.ProductPrice = productUpdates.ProductPrice.Value;

            if (!string.IsNullOrEmpty(productUpdates.ProductDescription))
                product.ProductDescription = productUpdates.ProductDescription;

            if (productUpdates.ProductQuantity.HasValue && productUpdates.ProductQuantity.Value > 0)
                product.ProductQuantity = productUpdates.ProductQuantity.Value;

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
                .Include(p => p.ProductImages) // Include images
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
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages) // Include images
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(p => p.Category.CategoryName == categoryName);
            }

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
                _ => query.OrderBy(p => p.ProductName)
            };

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
        public async Task<IActionResult> AddImageToProduct(int productId, [FromForm] List<IFormFile> imageFiles)
        {
            // Fetch the product
            var product = await _context.Products
                .Include(p => p.ProductImages) // Include existing images if necessary
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Check if any files are uploaded
            if (imageFiles == null || imageFiles.Count == 0)
            {
                return BadRequest("No files were uploaded.");
            }

            // Define the directory for storing product images
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                foreach (var file in imageFiles)
                {
                    if (file.Length > 0) // Check for non-empty file
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName)
                                        + "_" + Guid.NewGuid()
                                        + Path.GetExtension(file.FileName);

                        var filePath = Path.Combine(imageDirectory, fileName);

                        // Save the file to the directory
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add the new image to the database
                        var newImage = new ProductImage
                        {
                            ImagePath = fileName,
                            ProductId = productId
                        };

                        product.ProductImages.Add(newImage);
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok("Image(s) added successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception and return a proper error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
