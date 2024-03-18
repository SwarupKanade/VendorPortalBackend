using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyDocumentController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PolicyDocumentController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromForm] PolicyDocumentDto policyDocumentDto)
        {

            ValidateFileUpload(policyDocumentDto.Document);

            if (ModelState.IsValid)
            {
                string docPath = await Upload(policyDocumentDto.Document);

                var policyDocument = new PolicyDocument
                {
                    DocumentPath = docPath,
                    Name = policyDocumentDto.Name,
                    IsActive = policyDocumentDto.IsActive,
                    CreatedOn = DateTime.Now,
                    LastModifiedOn = DateTime.Now,
                };

                await dbContext.PolicyDocuments.AddAsync(policyDocument);
                await dbContext.SaveChangesAsync();
                return Ok(policyDocument);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var policyDocumentResult = await dbContext.PolicyDocuments.FirstOrDefaultAsync(x => x.Id == id);

            if (policyDocumentResult != null)
            {
                return Ok(policyDocumentResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            var policyDocumentResult = await dbContext.PolicyDocuments.ToListAsync();

            if (policyDocumentResult != null)
            {
                return Ok(policyDocumentResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("AllActive")]
        public async Task<IActionResult> GetActive()
        {
            var policyDocumentResult = await dbContext.PolicyDocuments.Where(x => x.IsActive).ToListAsync();

            if (policyDocumentResult != null)
            {
                return Ok(policyDocumentResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] PolicyDocumentUpdateDto policyDocumentUpdateDto)
        {
            var policyDocumentResult = await dbContext.PolicyDocuments.FirstOrDefaultAsync(x => x.Id == id);

            if (policyDocumentResult != null)
            {
                policyDocumentResult.Name = policyDocumentUpdateDto.Name;
                policyDocumentResult.IsActive = policyDocumentUpdateDto.IsActive;
                policyDocumentResult.LastModifiedOn = DateTime.Now;

                if (policyDocumentUpdateDto.Document != null)
                {
                    ValidateFileUpload(policyDocumentUpdateDto.Document);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(policyDocumentResult.DocumentPath);
                        if (del)
                        {
                            string docPath = await Upload(policyDocumentUpdateDto.Document);
                            policyDocumentResult.DocumentPath = docPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }

                }
                await dbContext.SaveChangesAsync();
                return Ok(policyDocumentResult);

            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var policyDocumentResult = await dbContext.PolicyDocuments.FirstOrDefaultAsync(x => x.Id == id);

            if (policyDocumentResult == null)
            {
                return NotFound();
            }

            dbContext.PolicyDocuments.Remove(policyDocumentResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        private async Task<string> Upload(IFormFile image)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "PolicyDocuments");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(image.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/PolicyDocuments/{uniqueName}{fileExt}";
            var FilePath = urlFilePath;
            return FilePath;
        }
        private void ValidateFileUpload(IFormFile image)
        {
            var allowedExtensions = new string[] { ".pdf" };

            if (!allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Unsupported file extension");
            }

            if (image.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size more than 10MB, please upload a smaller size file.");
            }
        }

        private bool Delete(string filePath)
        {
            if (filePath != null)
            {
                string[] files = filePath.Split("/");
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "PolicyDocuments", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}