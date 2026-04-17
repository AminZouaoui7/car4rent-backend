using Car4rentpg.DTOs;

namespace Car4rentpg.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileUploadService(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UploadVehicleImageResponseDto> UploadVehicleImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Aucun fichier envoyé.");

            if (file.Length > MaxFileSize)
                throw new Exception("Fichier trop volumineux. Maximum 5 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new Exception("Format invalide. Formats autorisés : jpg, jpeg, png, webp.");

            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var folderPath = Path.Combine(webRootPath, "uploads", "vehicles");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folderPath, safeFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext?.Request
                          ?? throw new Exception("Impossible de construire l’URL du fichier.");

            var imageUrl = $"{request.Scheme}://{request.Host}/uploads/vehicles/{safeFileName}";

            return new UploadVehicleImageResponseDto
            {
                Message = "Image uploadée avec succès.",
                ImageUrl = imageUrl,
                FileName = safeFileName
            };
        }

        public void DeleteVehicleImageIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                Uri uri;

                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out uri!))
                {
                    var fileNameFromAbsoluteUrl = Path.GetFileName(uri.LocalPath);
                    DeleteLocalVehicleFile(fileNameFromAbsoluteUrl);
                    return;
                }

                var fileNameFromRelativePath = Path.GetFileName(imageUrl);
                DeleteLocalVehicleFile(fileNameFromRelativePath);
            }
            catch
            {
                // on ignore si l’image n’est pas locale ou si l’URL est invalide
            }
        }

        private void DeleteLocalVehicleFile(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var filePath = Path.Combine(webRootPath, "uploads", "vehicles", fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}