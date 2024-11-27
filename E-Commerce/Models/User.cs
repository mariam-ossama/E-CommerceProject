using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace E_Commerce.Models
{
    public class User
    {
        [Key]
        [Required]
        public string UserId { get; set; }  // GUID as string (auto-generated)

        [MaxLength(50)]
        [NotNull]
        required public string UserName { get; set; }

        [MaxLength(255)]
        [NotNull]
        required public string Email { get; set; }

        public string? Gender { get; set; }

        required public string Password { get; set; }  // Store hashed password
        required public bool EmailConfirmed { get; set; }

        required public string Role { get; set; }  // Role as string (Customer, Merchant, Admin)

        public string? Token { get; set; }  // New token column

        // Navigation properties
        public Customer Customer { get; set; }
        public Employee Employee { get; set; }
        public Admin Admin { get; set; }

        public User()
        {
            UserId = Guid.NewGuid().ToString();  // Automatically generate GUID for UserId
        }
    }
}
