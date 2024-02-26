namespace VendorPortal.API.Models.DTO
{
    public class ProjectHeadUpdateDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
