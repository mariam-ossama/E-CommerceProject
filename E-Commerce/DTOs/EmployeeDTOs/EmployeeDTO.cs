namespace E_Commerce.DTOs.EmployeeDTOs
{
    public class EmployeeDTO
    {
        required public string UserId { get; set; }
        required public string UserName { get; set; }
        required public string UserEmail { get; set; }
        required public string Role { get; set; }
        public string? Gender { get; set; }
        public string? EmployeePosition { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? ProfilePicture { get; set; }
        public bool HasNullData { get; set; }
    }

    public class UpdateEmployeeDTO
    {
        required public string UserName { get; set; }
        public string? Gender { get; set; }
        public string? EmployeePosition { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
