namespace VendorPortal.API.Models.DTO
{
    public class PolicyDocumentDto
    {
        public string Name { get; set; }
        public IFormFile Document { get; set; }
        public bool IsActive { get; set; }
    }
}
