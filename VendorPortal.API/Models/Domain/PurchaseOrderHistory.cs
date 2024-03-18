using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VendorPortal.API.Models.Domain
{
    public class PurchaseOrderHistory
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsAccepted { get; set; }
        public string Comment { get; set; }

        [JsonIgnore]
        public PurchaseOrder PurchaseOrder { get; set; }
    }
}
