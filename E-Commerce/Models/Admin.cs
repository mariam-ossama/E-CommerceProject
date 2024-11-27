using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Admin
    {
        [Key]
        required public string UserId { get; set; }  // Foreign Key to User

        public string? AdminPrivileges { get; set; }  // Example privilege field

        // Navigation property
        required public User User { get; set; }
    }
}
