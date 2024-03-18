using VendorPortal.API.Models.Domain;

namespace VendorPortal.API.Models.DTO
{
    public class DocumentsUploadResponseDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string? DocumentPath { get; set; }
        public string? Comment { get; set; }
        public bool IsVerified { get; set; }

    }
}
