using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs
{
    public class RegisterDTO
    {
        [Required]
        required public string UserName { get; set; }

        [Required]
        [EmailAddress]
        required public string Email { get; set; }

        public string? Gender { get; set; }

        [Required]
        required public string Role { get; set; }

        [Required]
        [MinLength(8)]
        required public string Password { get; set; }

        [Required]
        required public string ConfirmPassword { get; set; }
    }
}
