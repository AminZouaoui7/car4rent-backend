namespace Car4rentpg.DTOs
{
    public class UploadVehicleImageResponseDto
    {
        public string Message { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}