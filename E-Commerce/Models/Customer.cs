using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Customer
    {
        [Key]
        required public string UserId { get; set; }  // Foreign Key to User

        public string? CustomerAddress { get; set; }
        public DateOnly? DOB { get; set; }
        [MaxLength(11)]
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }

        // Navigation property
        required public User User { get; set; }
    }
}
