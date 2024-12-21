using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public string UserId { get; set; }  // Foreign Key to User
        public int BasketId { get; set; }  // Reference to Basket/Order
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string PaymentMethod { get; set; }  // e.g., "Cash", "Credit Card", "Wallet"
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
    }
}
