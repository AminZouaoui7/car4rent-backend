using Microsoft.AspNetCore.Http;

namespace Car4rentpg.DTOs
{
    public class UploadVehicleImageRequestDto
    {
        public IFormFile File { get; set; } = null!;
    }
}