namespace VendorPortal.API.Models.Domain
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public DateTime EventDateTime { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
    }
}
