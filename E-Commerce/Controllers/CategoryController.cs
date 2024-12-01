using E_Commerce.Data;
using E_Commerce.DTOs.CategoryDTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCategory(CreateCategoryDTO dto)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var category = new Category
            {
                CategoryName = dto.CategoryName,
                Image = dto.Image
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok("Category added successfully.");
        }

        [HttpPatch("edit/{id}")]
        public async Task<IActionResult> EditCategory(int id, UpdateCategoryDTO dto)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Category not found.");

            if (!string.IsNullOrEmpty(dto.CategoryName))
                category.CategoryName = dto.CategoryName;

            if (!string.IsNullOrEmpty(dto.Image))
                category.Image = dto.Image;

            await _context.SaveChangesAsync();
            return Ok("Category updated successfully.");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == "Customer")
                return Forbid("Access denied.");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Category not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok("Category deleted successfully.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Category not found.");
            return Ok(category);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}
