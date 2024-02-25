using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {

        private readonly UserManager<UserProfile> userManager;

        public VendorController(UserManager<UserProfile> userManager)
        {
            this.userManager = userManager;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] VendorDto vendorDto)
        {

            var newVendor = new UserProfile
            {
                OrganizationName = vendorDto.OrganizationName,
                Name = vendorDto.Name,
                PhoneNumber = vendorDto.PhoneNumber,
                State = vendorDto.State,
                City = vendorDto.City,
                Address = vendorDto.Address,
                Pincode = vendorDto.Pincode,
                Email = vendorDto.Email,
                UserName = vendorDto.Email,
                VendorCategoryId = vendorDto.VendorCategoryId,
            };

            var vendorResult = await userManager.CreateAsync(newVendor, "Pass@123");

            if (vendorResult.Succeeded)
            {
                List<string> roles = new List<string>();
                    roles.Add("Vendor");
                    vendorResult = await userManager.AddToRolesAsync(newVendor, roles);

                    if (vendorResult.Succeeded)
                    {
                        return Ok("Vendor was registered! Please login.");
                    }
            }

            return BadRequest("Something went wrong");
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {

            var vendorResult = await userManager.FindByIdAsync(id);

           if (vendorResult != null)
           {
                var vendor = new VendorResponseDto
                {
                    Id = vendorResult.Id,
                    OrganizationName = vendorResult.OrganizationName,
                    Name = vendorResult.Name,
                    Email = vendorResult.Email,
                    PhoneNumber = vendorResult.PhoneNumber,
                    State = vendorResult.State,
                    Address = vendorResult.Address,
                    Pincode = (int)vendorResult.Pincode,
                    City = vendorResult.City,
                    VendorCategory = vendorResult.VendorCategory,
                };

        
               return Ok(vendor);
           }
            

            return BadRequest("Something went wrong");
        }


        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {

            var vendorResult = await userManager.GetUsersInRoleAsync("Vendor");

            if (vendorResult != null)
            {
                List<VendorResponseDto> allVendor = new List<VendorResponseDto>();

                foreach (var vendor in vendorResult)
                {
                    var newVendor = new VendorResponseDto
                    {
                        Id = vendor.Id,
                        OrganizationName = vendor.OrganizationName,
                        Name = vendor.Name,
                        Email = vendor.Email,
                        PhoneNumber = vendor.PhoneNumber,
                        State = vendor.State,
                        Address = vendor.Address,
                        Pincode = (int)vendor.Pincode,
                        City = vendor.City,
                        VendorCategory = vendor.VendorCategory,
                    };

                    allVendor.Add(newVendor);
                }

                return Ok(allVendor);
            }


            return BadRequest("Something went wrong");
        }


        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] VendorUpdateDto vendorUpdateDto)
        {

            var vendorResult = await userManager.FindByIdAsync(id);

            if (vendorResult != null)
            {

                if (vendorUpdateDto.NewPassword != "")
                {
                    var passResult = await userManager.ChangePasswordAsync(vendorResult, vendorUpdateDto.CurrentPassword, vendorUpdateDto.NewPassword);
                    if (!passResult.Succeeded)
                    {
                        return BadRequest(passResult.Errors);
                    }
                }
                vendorResult.OrganizationName = vendorUpdateDto.OrganizationName;
                vendorResult.Name = vendorUpdateDto.Name;
                vendorResult.PhoneNumber = vendorUpdateDto.PhoneNumber;
                vendorResult.City = vendorUpdateDto.City;
                vendorResult.State = vendorUpdateDto.State;
                vendorResult.Address = vendorUpdateDto.Address;
                vendorResult.Pincode = vendorUpdateDto.Pincode;
                
                await userManager.UpdateAsync(vendorResult);

                var vendor = new VendorResponseDto
                {
                    Id = vendorResult.Id,
                    OrganizationName = vendorResult.OrganizationName,
                    Name = vendorResult.Name,
                    PhoneNumber = vendorResult.PhoneNumber,
                    State = vendorResult.State,
                    Address = vendorResult.Address,
                    Pincode = (int)vendorResult.Pincode,
                    City = vendorResult.City,
                    VendorCategory = vendorResult.VendorCategory,
                };

                return Ok(vendor);
            }


            return BadRequest("Something went wrong");
        }

    }

}