using System.ComponentModel.DataAnnotations;

namespace VendorPortal.API.Models.DTO
{
    public class ProjectHeadLoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
