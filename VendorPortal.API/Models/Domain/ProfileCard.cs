namespace VendorPortal.API.Models.Domain
{
    public class ProfileCard
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Position { get; set; }
        public string ImagePath { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

        // Navigation Property
        public UserProfile UserProfile { get; set;}

    }
}
