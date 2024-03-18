using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortal.API.Data;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly VendorPortalDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PurchaseOrderController(VendorPortalDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromForm] PurchaseOrderDto purchaseOrderDto)
        {

            ValidateFileUpload(purchaseOrderDto.Document);

            if (ModelState.IsValid)
            {
                string docPath = await Upload(purchaseOrderDto.Document);

                var purchaseOrder = new PurchaseOrder
                {
                    OrderNo = purchaseOrderDto.OrderNo,
                    VendorId = purchaseOrderDto.VendorId,
                    ReleaseDate = DateTime.Now,
                    ExpectedDelivery = purchaseOrderDto.ExpectedDelivery,
                    OrderAmount = purchaseOrderDto.OrderAmount,
                    DocumentPath = docPath,
                    IsActive = purchaseOrderDto.IsActive,
                    CreatedOn = DateTime.Now,
                    LastModifiedOn = DateTime.Now,
                    PurchaseOrderHistories = new List<PurchaseOrderHistory>()
                };

                await dbContext.PurchaseOrders.AddAsync(purchaseOrder);
                await dbContext.SaveChangesAsync();
                return Ok(purchaseOrder);
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
            var purchaseOrderResult = await dbContext.PurchaseOrders.Include(x => x.PurchaseOrderHistories).FirstOrDefaultAsync(x => x.Id == id);

            if (purchaseOrderResult != null)
            {
                return Ok(purchaseOrderResult);
            }

            return BadRequest("Something went wrong");

        }


        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.Include(x => x.PurchaseOrderHistories).ToListAsync();

            if (purchaseOrderResult != null)
            {
                return Ok(purchaseOrderResult);
            }

            return BadRequest("Something went wrong");

        }

        [HttpGet]
        [Route("AllActive")]
        public async Task<IActionResult> GetActive()
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.Include(x => x.PurchaseOrderHistories).Where(x => x.IsActive).ToListAsync();

            if (purchaseOrderResult != null)
            {
                return Ok(purchaseOrderResult);
            }

            return BadRequest("Something went wrong");
        }


        [HttpGet]
        [Route("Vendor/{id:Guid}")]
        public async Task<IActionResult> GetByVendorId([FromRoute] string id)
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.Include(x => x.PurchaseOrderHistories).Where(x => x.IsActive && x.VendorId == id).ToListAsync();

            if (purchaseOrderResult != null)
            {
                return Ok(purchaseOrderResult);
            }

            return BadRequest("Something went wrong");
        }

        [HttpPut]
        [Route("AcceptReject/{id:Guid}")]
        public async Task<IActionResult> AcceptReject([FromRoute] Guid id, [FromBody] PurchaseOrderVendorUpdateDto purchaseOrderVendorUpdateDto)
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.Include(x => x.PurchaseOrderHistories).FirstOrDefaultAsync(x => x.Id == id);

            if (purchaseOrderResult != null && purchaseOrderResult.IsAccepted == null)
            {
                if (purchaseOrderVendorUpdateDto.IsAccepted)
                {
                    purchaseOrderResult.Accept(purchaseOrderVendorUpdateDto.Comment);
                    purchaseOrderResult.IsAccepted = purchaseOrderVendorUpdateDto.IsAccepted;
                    purchaseOrderResult.AcceptedOn = DateTime.Now;
                }
                else
                {
                    purchaseOrderResult.Reject(purchaseOrderVendorUpdateDto.Comment);
                    purchaseOrderResult.IsAccepted = purchaseOrderVendorUpdateDto.IsAccepted;
                }

                purchaseOrderResult.LastModifiedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();
                return Ok(purchaseOrderResult);
            }
            return BadRequest("Something went wrong");
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] PurchaseOrderUpdateDto purchaseOrderUpdateDto)
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.FirstOrDefaultAsync(x => x.Id == id);

            if (purchaseOrderResult != null)
            {
                purchaseOrderResult.OrderNo = purchaseOrderUpdateDto.OrderNo;
                purchaseOrderResult.VendorId = purchaseOrderUpdateDto.VendorId;
                purchaseOrderResult.ExpectedDelivery = purchaseOrderUpdateDto.ExpectedDelivery;
                purchaseOrderResult.OrderAmount = purchaseOrderUpdateDto.OrderAmount;
                purchaseOrderResult.IsActive = purchaseOrderUpdateDto.IsActive;
                purchaseOrderResult.LastModifiedOn = DateTime.Now;

                if (purchaseOrderUpdateDto.Document != null)
                {
                    ValidateFileUpload(purchaseOrderUpdateDto.Document);

                    if (ModelState.IsValid)
                    {
                        bool del = Delete(purchaseOrderResult.DocumentPath);
                        if (del)
                        {
                            string docPath = await Upload(purchaseOrderUpdateDto.Document);
                            purchaseOrderResult.DocumentPath = docPath;
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
                await dbContext.SaveChangesAsync();
                return Ok(purchaseOrderResult);

            }
            return BadRequest("Something went wrong");
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var purchaseOrderResult = await dbContext.PurchaseOrders.FirstOrDefaultAsync(x => x.Id == id);

            if (purchaseOrderResult == null)
            {
                return NotFound();
            }

            dbContext.PurchaseOrders.Remove(purchaseOrderResult);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }


        private async Task<string> Upload(IFormFile image)
        {
            var folder = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "PurchaseOrders");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string uniqueName = Guid.NewGuid().ToString();
            string fileExt = Path.GetExtension(image.FileName);
            var localFilePath = Path.Combine(folder, $"{uniqueName}{fileExt}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.CopyToAsync(stream);
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Files/PurchaseOrders/{uniqueName}{fileExt}";
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
                string ExitingFile = Path.Combine(webHostEnvironment.ContentRootPath, "Files", "PurchaseOrders", files[files.Length - 1]);
                System.IO.File.Delete(ExitingFile);
                return true;
            }
            return false;
        }

    }
}