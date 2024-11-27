using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace E_Commerce.Models
{
    public class Employee
    {
        [Key]
        required public string UserId { get; set; }  // Foreign Key to User

        public string? EmployeePosition { get; set; }  // Position of the employee
        public string? EmployeeDepartment { get; set; }  // Department they belong to

        // Nullable Profile Picture field
        public string? ProfilePicture { get; set; }  // Could be a URL or file path

        // Navigation property
        required public User User { get; set; }
    }
}
