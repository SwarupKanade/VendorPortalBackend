using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using VendorPortal.API.Mail;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {

        private readonly UserManager<UserProfile> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly EmailService emailService;

        public VendorController(UserManager<UserProfile> userManager, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor, EmailService emailService)
        {
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.emailService = emailService;
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
                DocumentPaths = "",
            };

            var vendorResult = await userManager.CreateAsync(newVendor, "Pass@123");

            if (vendorResult.Succeeded)
            {
                List<string> roles = new List<string>();
                    roles.Add("Vendor");
                    vendorResult = await userManager.AddToRolesAsync(newVendor, roles);

                    if (vendorResult.Succeeded)
                    {
                        SendWelcomeEmail(newVendor);
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
                    DocumentPaths = vendorResult.DocumentPaths,
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
                        DocumentPaths = vendor.DocumentPaths,
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
        public async Task<IActionResult> Update([FromRoute] string id, [FromForm] VendorUpdateDto vendorUpdateDto)
        {
            var vendorResult = await userManager.FindByIdAsync(id);

            if (vendorResult != null)
            {
                if (vendorUpdateDto.NewPassword != "" )
                {
                    var passResult = await userManager.ChangePasswordAsync(vendorResult, vendorUpdateDto.CurrentPassword, vendorUpdateDto.NewPassword);
                    if (!passResult.Succeeded)
                    {
                        return BadRequest(passResult.Errors);
                    }
                }

                if (vendorUpdateDto.Documents != null)
                {

                    string docPaths = "";

                    foreach (var doc in vendorUpdateDto.Documents)
                    {
                        ValidateFileUpload(doc);

                        if (ModelState.IsValid)
                        {
                            string docPath = await Upload(doc, id);
                            docPaths += (string.IsNullOrEmpty(docPaths) ? "" : ";") + docPath;
                            vendorResult.DocumentPaths = docPaths;
                        }
                        else { 
                            return BadRequest("Document File Error"); 
                        }
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
                    Email = vendorResult.Email,
                    PhoneNumber = vendorResult.PhoneNumber,
                    State = vendorResult.State,
                    Address = vendorResult.Address,
                    Pincode = (int)vendorResult.Pincode,
                    City = vendorResult.City,
                    VendorCategory = vendorResult.VendorCategory,
                    DocumentPaths = vendorResult.DocumentPaths,
                };

                return Ok(vendor);
            }


            return BadRequest("Something went wrong");
        }

        private async Task<string> Upload(IFormFile document, string id)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath,"VendorDocuments", id);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var localFilePath = Path.Combine(folder, document.FileName);

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await document.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/VendorDocuments/{id}/{document.FileName}";
            var FilePath = urlFilePath;
            return FilePath;
        }

        private void ValidateFileUpload(IFormFile document)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".pdf" };

            if (!allowedExtensions.Contains(Path.GetExtension(document.FileName)))
            {
                ModelState.AddModelError("file", "Unsupported file extension");
            }

            if (document.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size more than 10MB, please upload a smaller size file.");
            }
        }

        private void SendWelcomeEmail(UserProfile user)
        {
            string subject = $"Welcome to Our Application Vendor ID {user.Id}";
            string body = $"Dear {user.Name},\n\nWelcome to our application! Your username is: {user.UserName} and your password is: Pass@123\n\nBest regards,\nYour Application Team";

            emailService.SendEmail(user.Email, subject, body);
        }

    }

}