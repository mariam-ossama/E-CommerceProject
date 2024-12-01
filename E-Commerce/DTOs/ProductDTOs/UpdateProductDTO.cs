namespace E_Commerce.DTOs.ProductDTOs
{
    public class UpdateProductDTO
    {
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public List<string>? ProductImages { get; set; }
        public string? ProductDescription { get; set; }
        public int? ProductQuantity { get; set; }
    }
}
