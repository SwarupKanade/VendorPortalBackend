using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace VendorPortal.API.Models.Domain
{
    public class VendorCategory
    {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string? Description { get; set; }

            // Navigation property for many-to-many relationship
            public ICollection<VendorCategoryDocument> DocumentList { get; set; }
    }
}
