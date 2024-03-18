namespace VendorPortal.API.Models.DTO
{
    public class BannerUpdateDto
    {
        public IFormFile? Image { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
    }
}
