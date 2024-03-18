namespace VendorPortal.API.Models.Domain
{
    public class PolicyDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DocumentPath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
    }
}
