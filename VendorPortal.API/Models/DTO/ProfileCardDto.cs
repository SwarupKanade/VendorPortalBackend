namespace VendorPortal.API.Models.DTO
{
    public class ProfileCardDto
    {
        public string Description { get; set; }
        public string Position { get; set; }
        public IFormFile Image { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
