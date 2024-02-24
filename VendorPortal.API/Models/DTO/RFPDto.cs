using System.ComponentModel.DataAnnotations.Schema;

namespace VendorPortal.API.Models.DTO
{
    public class RFPDto
    {
        public string Title { get; set; }
        public IFormFile DocumentFile { get; set; }
        public Guid ProjectId { get; set; }
        public Guid VendorCategoryId { get; set; }
        public DateTime EndDate { get; set; }
    }
}
