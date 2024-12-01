using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;


// TODO: Testing
namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageImagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HomePageImagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-homepage-image")]
        [SwaggerOperation(Summary = "Add a new homepage image")]
        public async Task<IActionResult> AddHomepageImage([FromQuery] IFormFile imageFile, [FromQuery] string imageType, [FromQuery] int displayOrder)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            if (imageFile == null || string.IsNullOrEmpty(imageType))
            {
                return BadRequest("Image file and type are required.");
            }

            // Create folder for saving images if it doesn't exist
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "images", "homepage");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Save the image file to the server
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var savePath = Path.Combine(directoryPath, fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Add image metadata to the database
            var homepageImage = new HomePageImage
            {
                ImagePath = fileName,
                ImageType = imageType,
                DisplayOrder = displayOrder
            };

            _context.HomepageImages.Add(homepageImage);
            await _context.SaveChangesAsync();

            return Ok("Image added successfully.");
        }

        [HttpGet("homepage-images")]
        public async Task<IActionResult> GetHomepageImages()
        {
            var images = await _context.HomepageImages
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return Ok(images);
        }

        [HttpDelete("delete-homepage-image/{id}")]
        public async Task<IActionResult> DeleteHomepageImage(int id)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var homepageImage = await _context.HomepageImages.FindAsync(id);

            if (homepageImage == null)
            {
                return NotFound("Image not found.");
            }

            // Delete the image file from the server
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "images", "homepage", homepageImage.ImagePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            // Remove the image record from the database
            _context.HomepageImages.Remove(homepageImage);
            await _context.SaveChangesAsync();

            return Ok("Image deleted successfully.");
        }
    }
}