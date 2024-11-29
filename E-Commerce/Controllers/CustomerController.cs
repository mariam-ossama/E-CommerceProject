using E_Commerce.Data;
using E_Commerce.DTOs.CustomerDTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // Fetch all Customers
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == null)
                return Unauthorized("Unauthorized access: Authentication required.");

            if (role != "Admin")
                return Forbid("Access denied: Only admins can fetch all customers.");

            var customers = await _context.Customers
                .Include(c => c.User)
                .Select(c => new
                {
                    c.UserId,
                    c.User.UserName,
                    UserEmail = c.User.Email,
                    c.User.Role,
                    c.User.Gender,
                    c.CustomerAddress,
                    c.DOB,
                    c.PhoneNumber,
                    c.ProfilePicture,
                    HasNullData = c.CustomerAddress == null || c.DOB == null || c.PhoneNumber == null || c.ProfilePicture == null
                })
                .ToListAsync();

            return Ok(customers);
        }

        // Fetch Customer by userId
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCustomerById(string userId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.UserId,
                    c.User.UserName,
                    UserEmail = c.User.Email,
                    c.User.Role,
                    c.User.Gender,
                    c.CustomerAddress,
                    c.DOB,
                    c.PhoneNumber,
                    c.ProfilePicture,
                    HasNullData = c.CustomerAddress == null || c.DOB == null || c.PhoneNumber == null || c.ProfilePicture == null
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound("Customer not found.");

            return Ok(customer);
        }


        // Update the current Customer by Token
        [HttpPatch("{userId}")]
        public async Task<IActionResult> UpdateCustomer(string userId, [FromBody] UpdateCustomerDTO updatedCustomer)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            var customer = await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
                return NotFound("Customer not found.");

            if (role != "Admin")
                return Forbid("Access denied: Only admins can update customer records.");

            if (updatedCustomer.CustomerAddress != null)
                customer.CustomerAddress = updatedCustomer.CustomerAddress;

            if (updatedCustomer.DOB != null)
                customer.DOB = updatedCustomer.DOB;

            if (updatedCustomer.PhoneNumber != null)
                customer.PhoneNumber = updatedCustomer.PhoneNumber;

            if (updatedCustomer.ProfilePicture != null)
                customer.ProfilePicture = updatedCustomer.ProfilePicture;

            if (updatedCustomer.Gender != null)
                customer.User.Gender = updatedCustomer.Gender;

            if (updatedCustomer.UserName != null && customer.User != null)
                customer.User.UserName = updatedCustomer.UserName;

            await _context.SaveChangesAsync();
            return Ok("Customer updated successfully.");
        }




        // This is for Admin
        // Delete the current Customer by Token
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteCustomer(string userId)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == null || role != "Admin")
                return Forbid("Access denied: Only admins can delete customers.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
                return NotFound("Customer not found.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok("Customer deleted successfully.");
        }

    }
}
