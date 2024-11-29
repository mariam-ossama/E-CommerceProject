using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        required public string Email { get; set; }

        [Required]
        required public string Password { get; set; }
    }
}
