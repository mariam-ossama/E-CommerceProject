using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Create a new Customer
        [HttpPost("create")]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (await _context.Customers.AnyAsync(c => c.UserId == customer.UserId))
                return BadRequest("Customer already exists.");

            var user = await _context.Users.FindAsync(customer.UserId);
            if (user == null)
                return NotFound("User not found.");

            customer.User = user;
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return Ok("Customer created successfully.");
        }

        // Fetch all Customers
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Select(c => new
                {
                    c.UserId,
                    c.CustomerAddress,
                    c.DOB,
                    c.PhoneNumber,
                    c.ProfilePicture,
                    HasNullData = c.CustomerAddress == null || c.DOB == null || c.PhoneNumber == null || c.ProfilePicture == null
                })
                .ToListAsync();

            return Ok(customers);
        }

        // Fetch a specific Customer by UserId
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCustomerById(string userId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.UserId,
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

        // Update Customer
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateCustomer(string userId, [FromBody] Customer updatedCustomer)
        {
            var customer = await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
                return NotFound("Customer not found.");

            if (updatedCustomer.CustomerAddress != null)
                customer.CustomerAddress = updatedCustomer.CustomerAddress;

            if (updatedCustomer.DOB != null)
                customer.DOB = updatedCustomer.DOB;

            if (updatedCustomer.PhoneNumber != null)
                customer.PhoneNumber = updatedCustomer.PhoneNumber;

            if (updatedCustomer.ProfilePicture != null)
                customer.ProfilePicture = updatedCustomer.ProfilePicture;

            await _context.SaveChangesAsync();
            return Ok("Customer updated successfully.");
        }

        // Delete Customer
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteCustomer(string userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
                return NotFound("Customer not found.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok("Customer deleted successfully.");
        }
    }
}