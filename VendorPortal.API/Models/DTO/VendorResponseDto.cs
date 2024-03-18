﻿using System.ComponentModel.DataAnnotations;
using VendorPortal.API.Models.Domain;

namespace VendorPortal.API.Models.DTO
{
    public class VendorResponseDto
    {
        public string Id { get; set; }
        public string OrganizationName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Pincode { get; set; }
        public bool IsVerified { get; set; }

        public ICollection<DocumentsUploadResponseDto>? DocumentsUploadList { get; set; }

        public VendorCategory VendorCategory { get; set; }

    }
}
