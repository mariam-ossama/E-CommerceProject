namespace E_Commerce.DTOs.AuthDTOs
{
    public class ResetPasswordDTO
    {
        required public string Token { get; set; }
        required public string NewPassword { get; set; }
        required public string ConfirmPassword { get; set; }
    }
}
