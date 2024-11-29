namespace E_Commerce.DTOs.CustomerDTOs
{
    public class UpdateCustomerDTO
    {
        public string? CustomerAddress { get; set; }
        public DateOnly? DOB { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Gender { get; set; }
        public string? UserName { get; set; }
    }
}
