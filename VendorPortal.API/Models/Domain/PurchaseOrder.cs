namespace VendorPortal.API.Models.Domain
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public int OrderNo { get; set; }
        public string VendorId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime ExpectedDelivery { get; set; }
        public string DocumentPath { get; set; }
        public int OrderAmount { get; set; }
        public string? TotalGRN { get; set; }
        public string? Invoice { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

        // Navigation Property
        public UserProfile Vendor { get; set; }
        public ICollection<PurchaseOrderHistory> PurchaseOrderHistories { get; set; }

        public void Accept(string comment)
        {
            IsAccepted = true;
            PurchaseOrderHistories.Add(new PurchaseOrderHistory
            {
                PurchaseOrderId = Id,
                IsAccepted = true,
                Comment = comment,
                TimeStamp = DateTime.Now,
            });
        }

        public void Reject(string comment)
        {
            IsAccepted = false;
            PurchaseOrderHistories.Add(new PurchaseOrderHistory
            {
                PurchaseOrderId = Id,
                IsAccepted = false,
                Comment = comment,
                TimeStamp = DateTime.Now,
            });
        }
    }
}
