namespace VendorPortal.API.Models.Domain
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation property for many-to-many relationship
        public ICollection<VendorCategoryDocument> VendorCategories { get; set; }
    }
}
