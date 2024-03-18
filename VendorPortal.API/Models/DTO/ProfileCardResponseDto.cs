using VendorPortal.API.Models.Domain;

namespace VendorPortal.API.Models.DTO
{
    public class ProfileCardResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Position { get; set; }
        public string ImagePath { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

    }
}
