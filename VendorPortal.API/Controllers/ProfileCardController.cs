using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileCardController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ProfileCardController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromForm] ProfileCardDto profileCardDto)
        {

            ValidateFileUpload(profileCardDto.Image);

            if (ModelState.IsValid)
            {
                string imgPath = await Upload(profileCardDto.Image);

                var newprofileCard = new ProfileCard
                {
                    Description = profileCardDto.Description,
                    Position = profileCardDto.Position,
                    ImagePath = imgPath,
                    UserId = profileCardDto.UserId,
                    IsActive = profileCardDto.IsActive,
                    CreatedOn = DateTime.Now,
                    LastModifiedOn = DateTime.Now,
                };

                await dbContext.ProfileCards.AddAsync(newprofileCard);
                await dbContext.SaveChangesAsync();

                var profileCard = new ProfileCardResponseDto
                {
                    Id = newprofileCard.Id,
                    Description = newprofileCard.Description,
                    Position = newprofileCard.Position,
                    ImagePath = newprofileCard.ImagePath,
                    UserId = newprofileCard.UserId,
                    IsActive = newprofileCard.IsActive,
                    CreatedOn = newprofileCard.CreatedOn,
                    LastModifiedOn = newprofileCard.LastModifiedOn,
                };
                return Ok(profileCard);
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
            var profileCardResult = await dbContext.ProfileCards.FirstOrDefaultAsync(x => x.Id == id);

            if (profileCardResult != null)
            {
                return Ok(profileCardResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            var profileCardResult = await dbContext.ProfileCards.Include("UserProfile").ToListAsync();

            if (profileCardResult != null)
            {
                List<ProfileCardResponseDto> allProfileCard = new List<ProfileCardResponseDto>();
                foreach(var profileCard in profileCardResult)
                {
                    var newProfileCard = new ProfileCardResponseDto
                    {
                        Id = profileCard.Id,
                        Name = profileCard.UserProfile.Name,
                        Email = profileCard.UserProfile.Email,
                        Description = profileCard.Description,
                        Position = profileCard.Position,
                        ImagePath = profileCard.ImagePath,
                        UserId = profileCard.UserId,
                        IsActive = profileCard.IsActive,
                        CreatedOn = profileCard.CreatedOn,
                        LastModifiedOn = profileCard.LastModifiedOn,
                    };
                    allProfileCard.Add(newProfileCard);
                }
                return Ok(allProfileCard);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("AllActive")]
        public async Task<IActionResult> GetActive()
        {
            var profileCardResult = await dbContext.ProfileCards.Include("UserProfile").Where(x => x.IsActive).ToListAsync();

            if (profileCardResult != null)
            {
                List<ProfileCardResponseDto> allProfileCard = new List<ProfileCardResponseDto>();
                foreach (var profileCard in profileCardResult)
                {
                    var newProfileCard = new ProfileCardResponseDto
                    {
                        Id = profileCard.Id,
                        Name = profileCard.UserProfile.Name,
                        Email = profileCard.UserProfile.Email,
                        Description = profileCard.Description,
                        Position = profileCard.Position,
                        ImagePath = profileCard.ImagePath,
                        UserId = profileCard.UserId,
                        IsActive = profileCard.IsActive,
                        CreatedOn = profileCard.CreatedOn,
                        LastModifiedOn = profileCard.LastModifiedOn,
                    };
                    allProfileCard.Add(newProfileCard);
                }
                return Ok(allProfileCard);
            }

            return BadRequest("Something went wrong");

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] ProfileCardUpdateDto profileCardUpdateDto)
        {
            var profileCardResult = await dbContext.ProfileCards.FirstOrDefaultAsync(x => x.Id == id);

            if (profileCardResult != null)
            {
                profileCardResult.Description = profileCardUpdateDto.Description;
                profileCardResult.Position = profileCardUpdateDto.Position;
                profileCardResult.IsActive = profileCardUpdateDto.IsActive;
                profileCardResult.UserId = profileCardUpdateDto.UserId;
                profileCardResult.LastModifiedOn = DateTime.Now;

                if (profileCardUpdateDto.Image != null)
                {
                    ValidateFileUpload(profileCardUpdateDto.Image);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(profileCardResult.ImagePath);
                        if (del)
                        {
                            string imgPath = await Upload(profileCardUpdateDto.Image);
                            profileCardResult.ImagePath = imgPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }

                }
                await dbContext.SaveChangesAsync();

                var profileCard = new ProfileCardResponseDto
                {
                    Id = profileCardResult.Id,
                    Description = profileCardResult.Description,
                    Position = profileCardResult.Position,
                    ImagePath = profileCardResult.ImagePath,
                    UserId = profileCardResult.UserId,
                    IsActive = profileCardResult.IsActive,
                    CreatedOn = profileCardResult.CreatedOn,
                    LastModifiedOn = profileCardResult.LastModifiedOn,
                };
                return Ok(profileCard);

            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var profileCardResult = await dbContext.ProfileCards.FirstOrDefaultAsync(x => x.Id == id);

            if (profileCardResult == null)
            {
                return NotFound();
            }

            dbContext.ProfileCards.Remove(profileCardResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        private async Task<string> Upload(IFormFile image)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "ProfileCards");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(image.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/ProfileCards/{uniqueName}{fileExt}";
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
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "ProfileCards", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}