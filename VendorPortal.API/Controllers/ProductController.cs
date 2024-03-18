using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        public ProductController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }


        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromForm] ProductDto productDto)
        {

            ValidateFileUpload(productDto.ImageFile);
            if (ModelState.IsValid)
            {
                string imagePath = await Upload(productDto.ImageFile);

                var productDomain = new Product
                {
                    Name = productDto.Name,
                    ImagePath = imagePath,
                    LongDescription = productDto.LongDescription,
                    ShortDescription = productDto.ShortDescription,
                    UnitType = productDto.UnitType,
                    Size = productDto.Size,
                    Specification = productDto.Specification,
                    CategoryId = productDto.ProductCategoryId,
                    SubCategoryId = productDto.SubCategoryId,
                };

                await dbContext.Products.AddAsync(productDomain);
                await dbContext.SaveChangesAsync();
                return Ok(productDomain);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterVal)
        {
            var productsResult = dbContext.Products.Include("Category").Include("SubCategory").AsQueryable();

            if (String.IsNullOrWhiteSpace(filterOn) == false && String.IsNullOrWhiteSpace(filterVal) == false)
            {
                if (filterOn.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    productsResult = productsResult.Where(x => x.Name.ToLower().Contains(filterVal.ToLower()));
                }

                if (filterOn.Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    productsResult = productsResult.Where(x => x.Category.Name.ToLower().Contains(filterVal.ToLower())); 
                }

                if (filterOn.Equals("subcategory", StringComparison.OrdinalIgnoreCase))
                {
                    productsResult = productsResult.Where(x => x.SubCategory.Name.ToLower().Contains(filterVal.ToLower()));
                }
            }

            if (productsResult != null)
            {
                List<ProductResponseDto> allproducts = new List<ProductResponseDto>();
                foreach (var product in productsResult)
                {
                    var productResponseDto = new ProductResponseDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        ImagePath = product.ImagePath,
                        ShortDescription = product.ShortDescription,
                        LongDescription = product.LongDescription,
                        UnitType = product.UnitType,
                        Size = product.Size,
                        Category = product.Category.Name,
                        CategoryId = product.CategoryId,
                        SubCategory = product.SubCategory.Name,
                        SubCategoryId = product.SubCategoryId,
                        Specification = product.Specification,
                    };
                    allproducts.Add(productResponseDto);
                }
                return Ok(allproducts);
            }

            return BadRequest("Something went wrong");
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var productResult = await dbContext.Products.Include("Category").Include("SubCategory").FirstOrDefaultAsync(x => x.Id == id);

            if (productResult != null)
            {
                var productResponseDto = new ProductResponseDto
                {
                    Id = productResult.Id,
                    Name = productResult.Name,
                    ImagePath = productResult.ImagePath,
                    ShortDescription = productResult.ShortDescription,
                    LongDescription = productResult.LongDescription,
                    UnitType = productResult.UnitType,
                    Size = productResult.Size,
                    Category = productResult.Category.Name,
                    CategoryId = productResult.CategoryId,
                    SubCategory = productResult.SubCategory.Name,
                    SubCategoryId = productResult.SubCategoryId,
                    Specification = productResult.Specification,
                };
                return Ok(productResponseDto);
            }

            return BadRequest("Something went wrong");
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] ProductUpdateDto productUpdateDto)
        {
            var productResult = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (productResult != null)
            {
                productResult.Name = productUpdateDto.Name;
                productResult.ShortDescription = productUpdateDto.ShortDescription;
                productResult.LongDescription = productUpdateDto.LongDescription;
                productResult.UnitType = productUpdateDto.UnitType;
                productResult.Size = productUpdateDto.Size;
                productResult.Specification = productUpdateDto.Specification;
                productResult.CategoryId = productUpdateDto.ProductCategoryId;
                productResult.SubCategoryId = productUpdateDto.SubCategoryId;

                if (productUpdateDto.ImageFile != null)
                {
                    ValidateFileUpload(productUpdateDto.ImageFile);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(productResult.ImagePath);
                        if (del)
                        {
                            string imgPath = await Upload(productUpdateDto.ImageFile);
                            productResult.ImagePath = imgPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }

                }
                await dbContext.SaveChangesAsync();
                return Ok(productResult);
            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var productResult = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (productResult == null)
            {
                return NotFound();
            }

            dbContext.Products.Remove(productResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string> Upload(IFormFile document)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "ProductImages");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(document.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await document.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/ProductImages/{uniqueName}{fileExt}";
            var FilePath = urlFilePath;
            return FilePath;
        }

        private void ValidateFileUpload(IFormFile document)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(document.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Unsupported file extension");
            }

            if (document.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size more than 10MB, please upload a smaller size file.");
            }
        }

        private bool Delete(string filePath)
        {
            if (filePath != null)
            {
                string[] files = filePath.Split("/");
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "ProductImages", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}
