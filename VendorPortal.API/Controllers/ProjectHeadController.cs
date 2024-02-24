using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VendorPortal.API.Models.Domain;
using VendorPortal.API.Models.DTO;

namespace VendorPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectHeadController : ControllerBase
    {

        private readonly UserManager<UserProfile> userManager;

        public ProjectHeadController(UserManager<UserProfile> userManager)
        {
            this.userManager = userManager;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] ProjectHeadDto projectHeadDto)
        {

            var newProjectHead = new UserProfile
            {
               
                Name = projectHeadDto.Name,
                PhoneNumber = projectHeadDto.PhoneNumber,
                Email = projectHeadDto.Email,
                UserName = projectHeadDto.UserName,
            };

            var projectHeadResult = await userManager.CreateAsync(newProjectHead, "Pass@123");

            if (projectHeadResult.Succeeded)
            {
                List<string> roles = new List<string>();
                roles.Add("ProjectHead");
                projectHeadResult = await userManager.AddToRolesAsync(newProjectHead, roles);

                if (projectHeadResult.Succeeded)
                {
                    return Ok("ProjectHead was registered! Please login.");
                }
            }

            return BadRequest("Something went wrong");
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {

            var projectHeadResult = await userManager.FindByIdAsync(id);

            if (projectHeadResult != null)
            {
                var projectHead = new ProjectHeadResponseDto
                {
                    Id = projectHeadResult.Id,
                    Name = projectHeadResult.Name,
                    PhoneNumber = projectHeadResult.PhoneNumber,

                };

                return Ok(projectHead);
            }


            return BadRequest("Something went wrong");
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ProjectHeadUpdateDto projectHeadUpdateDto)
        {

            var projectHeadResult = await userManager.FindByIdAsync(id);

            if (projectHeadResult != null)
            {
                projectHeadResult.Name = projectHeadUpdateDto.Name;
                projectHeadResult.PhoneNumber = projectHeadUpdateDto.PhoneNumber;

                await userManager.ChangePasswordAsync(projectHeadResult, projectHeadUpdateDto.CurrentPassword, projectHeadUpdateDto.Password);


                await userManager.UpdateAsync(projectHeadResult);

                var projectHead = new ProjectHeadResponseDto
                {
                    Id = projectHeadResult.Id,
                    Name = projectHeadResult.Name,
                    PhoneNumber = projectHeadResult.PhoneNumber,

                };

                return Ok(projectHead);
            }


            return BadRequest("Something went wrong");
        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {

            var projectHeadsResult = await userManager.GetUsersInRoleAsync("ProjectHead");

            if (projectHeadsResult != null)
            {
                List<ProjectHeadResponseDto> allProjectHead = new List<ProjectHeadResponseDto>();

                foreach (var projectHead in projectHeadsResult)
                {
                    var newProjectHead = new ProjectHeadResponseDto
                    {
                        Id = projectHead.Id,
                        Name = projectHead.Name,
                        PhoneNumber = projectHead.PhoneNumber,

                    };

                    allProjectHead.Add(newProjectHead);
                }
                
                return Ok(allProjectHead);
            }


            return BadRequest("Something went wrong");
        }
    }
}