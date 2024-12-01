using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace E_Commerce.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        // Change List<string> to a collection of Image entities
        public List<ProductImage>? ProductImages { get; set; } = new();

        public string? ProductDescription { get; set; }

        [Required]
        public int ProductQuantity { get; set; }

        // Navigation property
        public Category Category { get; set; }
    }
}
