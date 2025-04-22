using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WageWise.Application.Interfaces.Services;
using WageWise.Infrastructure.Services;

namespace WageWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly ICVService _cvService;

        public CVController(ICVService cvService)
        {
            _cvService = cvService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string province, [FromForm] string? district = null)
        {
            var result = await _cvService.HandleAsync(file, province, district);

            if (result.IsFailure)
            {

                return BadRequest(new
                {
                    success = false,
                    error = result.Error
                });
            }

            return Ok(new
            {
                success = true,
                data = result.Value
            });
        }
    }
}
