using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BannerController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromForm] BannerDto bannerDto)
        {

            ValidateFileUpload(bannerDto.Image);

            if (ModelState.IsValid)
            {
                string imgPath = await Upload(bannerDto.Image);

                var banner = new Banner
                {
                    ImagePath = imgPath,
                    Title = bannerDto.Title,
                    IsActive = bannerDto.IsActive,
                    CreatedOn = DateTime.Now,
                    LastModifiedOn = DateTime.Now,
                };

                await dbContext.Banners.AddAsync(banner);
                await dbContext.SaveChangesAsync();
                return Ok(banner);
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
            var bannerResult = await dbContext.Banners.FirstOrDefaultAsync(x => x.Id == id);

            if (bannerResult != null)
            {
                return Ok(bannerResult);
            }

            return BadRequest("Something went wrong");

        }


        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            var bannerResult = await dbContext.Banners.ToListAsync();

            if (bannerResult != null)
            {
                return Ok(bannerResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("AllActive")]
        public async Task<IActionResult> GetActive()
        {
            var bannerResult = await dbContext.Banners.Where(x => x.IsActive).ToListAsync();

            if (bannerResult != null)
            {
                return Ok(bannerResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] BannerUpdateDto bannerUpdateDto)
        {
            var bannerResult = await dbContext.Banners.FirstOrDefaultAsync(x => x.Id == id);

            if (bannerResult != null)
            {
                bannerResult.Title = bannerUpdateDto.Title;
                bannerResult.IsActive = bannerUpdateDto.IsActive;
                bannerResult.LastModifiedOn = DateTime.Now;

                if (bannerUpdateDto.Image != null)
                {
                    ValidateFileUpload(bannerUpdateDto.Image);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(bannerResult.ImagePath);
                        if (del)
                        {
                            string imgPath = await Upload(bannerUpdateDto.Image);
                            bannerResult.ImagePath = imgPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }

                }
                await dbContext.SaveChangesAsync();
                return Ok(bannerResult);

            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var bannerResult = await dbContext.Banners.FirstOrDefaultAsync(x => x.Id == id);

            if (bannerResult == null)
            {
                return NotFound();
            }

            dbContext.Banners.Remove(bannerResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        private async Task<string> Upload(IFormFile image)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "Banners");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(image.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/Banners/{uniqueName}{fileExt}";
            var FilePath = urlFilePath;
            return FilePath;
        }
        private void ValidateFileUpload(IFormFile image)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

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
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "Banners", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}