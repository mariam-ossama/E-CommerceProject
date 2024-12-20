using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class BasketItem
    {
        [Key]
        public int BasketItemId { get; set; }

        public string UserId { get; set; }  // Foreign Key to User
        public int ProductId { get; set; }  // Foreign Key to Product

        [Required]
        public int Quantity { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
