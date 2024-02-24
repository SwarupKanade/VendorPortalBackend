using System.ComponentModel.DataAnnotations;

namespace VendorPortal.API.Models.DTO
{
    public class VendorLoginDto
    {
        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage ="Invalid Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password, ErrorMessage = "Invalid Password")]
        public string Password { get; set; }
    }
}
