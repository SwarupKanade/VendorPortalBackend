using System.Text.Json.Serialization;

namespace VendorPortal.API.Models.Domain
{
    public class VendorCategoryDocument
    {
        public Guid DocumentId { get; set; }
        [JsonIgnore]
        public Document Document { get; set; }

        public Guid VendorCategoryId { get; set; }
        [JsonIgnore]
        public VendorCategory VendorCategory { get; set; }
    }
}
