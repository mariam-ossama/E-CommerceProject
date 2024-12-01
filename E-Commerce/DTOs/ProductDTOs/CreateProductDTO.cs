namespace E_Commerce.DTOs.ProductDTOs
{
    public class CreateProductDTO
    {
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public decimal ProductPrice { get; set; }
        public List<string> ProductImages { get; set; } = new(); // Image paths as strings
        public string? ProductDescription { get; set; }
        public int ProductQuantity { get; set; }
    }
}
