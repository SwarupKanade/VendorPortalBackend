namespace VendorPortal.API.Models.DTO
{
    public class VendorDocUpdateDto
    {
        public Guid Id { get; set; }
        public IFormFile Document { get; set; }

    }
}
