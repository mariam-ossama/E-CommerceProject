namespace E_Commerce.DTOs.CategoryDTOs
{
    public class CreateCategoryDTO
    {
        required public string CategoryName { get; set; }
        public string? Image { get; set; }
    }
}
