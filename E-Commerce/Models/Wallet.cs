using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        public string UserId { get; set; }  // Foreign Key to User
        [Required]
        public decimal Balance { get; set; } = 0;

        // Navigation property
        public User User { get; set; }
    }
}
