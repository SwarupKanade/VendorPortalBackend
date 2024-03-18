using System.ComponentModel.DataAnnotations.Schema;

namespace VendorPortal.API.Models.Domain
{
    public class RFP
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Document { get; set; }
        public Guid ProjectId { get; set; }
        public Guid VendorCategoryId { get; set; }
        public DateTime EndDate { get;set; }

        [NotMapped]
        public bool IsActive => EndDate > DateTime.Now;

        // Navigation properties
        public VendorCategory VendorCategory { get; set; }
        public Project Project { get; set; }


    }
}
