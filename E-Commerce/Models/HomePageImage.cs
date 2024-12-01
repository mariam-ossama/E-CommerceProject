namespace E_Commerce.Models
{
    public class HomePageImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } // Path to the stored image
        public string ImageType { get; set; } // e.g., "Bestseller", "NewArrival"
        public int DisplayOrder { get; set; } // For sorting images on the homepage
    }
}
