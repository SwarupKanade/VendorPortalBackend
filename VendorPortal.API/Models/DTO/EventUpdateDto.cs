namespace VendorPortal.API.Models.DTO
{
    public class EventUpdateDto
    {
        public string Title { get; set; }
        public IFormFile? Image { get; set; }
        public DateTime EventDateTime { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }
}
