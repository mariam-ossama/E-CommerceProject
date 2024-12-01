namespace E_Commerce.DTOs.CategoryDTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        required public string CategoryName { get; set; }
        public string? Image { get; set; }
    }
}
