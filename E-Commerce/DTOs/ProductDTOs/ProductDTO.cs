namespace E_Commerce.DTOs.ProductDTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal ProductPrice { get; set; }
        public List<string> ProductImages { get; set; } = new(); // Image paths as strings
        public string? ProductDescription { get; set; }
        public int ProductQuantity { get; set; }
    }
}
