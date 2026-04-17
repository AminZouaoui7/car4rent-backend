using Car4rentpg.DTOs;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Authorize(Roles = "Admin")]
    public class UploadsController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;

        public UploadsController(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("vehicle-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadVehicleImage([FromForm] UploadVehicleImageRequestDto request)
        {
            try
            {
                if (request.File == null)
                {
                    return BadRequest(new
                    {
                        message = "Aucun fichier envoyé."
                    });
                }

                var result = await _fileUploadService.UploadVehicleImageAsync(request.File);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}