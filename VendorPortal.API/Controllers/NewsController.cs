using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public NewsController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromForm] NewsDto newsDto)
        {

            ValidateFileUpload(newsDto.Image);

            if (ModelState.IsValid)
            {
                string imgPath = await Upload(newsDto.Image);

                var news = new News
                {
                    Title = newsDto.Title,
                    ImagePath = imgPath,
                    Content = newsDto.Content,
                    IsActive = newsDto.IsActive,
                    CreatedOn = DateTime.Now,
                    LastModifiedOn = DateTime.Now,
                };

                await dbContext.Newss.AddAsync(news);
                await dbContext.SaveChangesAsync();
                return Ok(news);
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
            var newsResult = await dbContext.Newss.FirstOrDefaultAsync(x => x.Id == id);

            if (newsResult != null)
            {
                return Ok(newsResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            var newsResult = await dbContext.Newss.ToListAsync();

            if (newsResult != null)
            {
                return Ok(newsResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("AllActive")]
        public async Task<IActionResult> GetActive()
        {
            var newsResult = await dbContext.Newss.Where(x => x.IsActive).ToListAsync();

            if (newsResult != null)
            {
                return Ok(newsResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] NewsUpdateDto newsUpdateDto)
        {
            var newsResult = await dbContext.Newss.FirstOrDefaultAsync(x => x.Id == id);

            if (newsResult != null)
            {
                newsResult.Title = newsUpdateDto.Title;
                newsResult.Content = newsUpdateDto.Content;
                newsResult.IsActive = newsUpdateDto.IsActive;
                newsResult.LastModifiedOn = DateTime.Now;

                if (newsUpdateDto.Image != null)
                {
                    ValidateFileUpload(newsUpdateDto.Image);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(newsResult.ImagePath);
                        if (del)
                        {
                            string imgPath = await Upload(newsUpdateDto.Image);
                            newsResult.ImagePath = imgPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }

                }
                await dbContext.SaveChangesAsync();
                return Ok(newsResult);

            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var newsResult = await dbContext.Newss.FirstOrDefaultAsync(x => x.Id == id);

            if (newsResult == null)
            {
                return NotFound();
            }

            dbContext.Newss.Remove(newsResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        private async Task<string> Upload(IFormFile image)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "News");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(image.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/News/{uniqueName}{fileExt}";
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
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "News", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}