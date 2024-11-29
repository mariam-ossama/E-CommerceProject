using E_Commerce.Data;
using E_Commerce.DTOs.EmployeeDTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // Fetch all Employees (Admin and Employee roles)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == null)
                return Unauthorized("Unauthorized access: Authentication required.");

            if (role != "Admin")
                return Forbid("Access denied: Only admins can fetch all customers.");

            var employees = await _context.Employees
                .Include(e => e.User)
                .Select(e => new EmployeeDTO
                {
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    UserEmail = e.User.Email,
                    Role = e.User.Role,
                    Gender = e.User.Gender,
                    EmployeePosition = e.EmployeePosition,
                    EmployeeDepartment = e.EmployeeDepartment,
                    ProfilePicture = e.ProfilePicture,
                    HasNullData = e.EmployeePosition == null || e.EmployeeDepartment == null || e.ProfilePicture == null || e.User.Gender == null
                })
                .ToListAsync();

            return Ok(employees);
        }

        // Fetch the current Employee by ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetEmployeeById(string userId)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();

            if (role == "Customer")
            {
                return Forbid("Access denied: Only admins or employees can fetch employee data.");
            }
            var employee = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .Select(e => new
                {
                    e.UserId,
                    e.User.UserName,
                    UserEmail = e.User.Email,
                    e.User.Role,
                    e.User.Gender,
                    e.EmployeePosition,
                    e.EmployeeDepartment,
                    e.ProfilePicture,
                    HasNullData = e.EmployeePosition == null || e.EmployeeDepartment == null || e.ProfilePicture == null || e.User.Gender == null
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound("Employee not found.");

            return Ok(employee);
        }

        // Update the current Employee by ID
        [HttpPatch("update")]
        public async Task<IActionResult> UpdateEmployee([FromQuery] string userId,[FromBody] UpdateEmployeeDTO updatedEmployee)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();

            var employee = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null)
                return NotFound("Employee not found.");

            if (role == "Customer" && employee.UserId != userId )
            {
                return Forbid("Access denied: Only admins or the employee can update this record.");
            }

            if (updatedEmployee.EmployeePosition != null)
                employee.EmployeePosition = updatedEmployee.EmployeePosition;

            if (updatedEmployee.EmployeeDepartment != null)
                employee.EmployeeDepartment = updatedEmployee.EmployeeDepartment;


            if (updatedEmployee.ProfilePicture != null)
                employee.ProfilePicture = updatedEmployee.ProfilePicture;

            if (updatedEmployee.Gender != null)
                employee.User.Gender = updatedEmployee.Gender;

            if (updatedEmployee.UserName != null && employee.User != null)
                employee.User.UserName = updatedEmployee.UserName;

            await _context.SaveChangesAsync();
            return Ok("Employee updated successfully.");
        }

        // Delete an Employee (Admin only)
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteEmployee([FromQuery] string userIdToDelete)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == null)
                return Unauthorized("Unauthorized access: Authentication required.");

            if (role != "Admin")
                return Forbid("Access denied: Only admins can delete Employees.");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userIdToDelete);
            if (employee == null)
                return NotFound("Employee not found.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee deleted successfully.");
        }
    }
}
