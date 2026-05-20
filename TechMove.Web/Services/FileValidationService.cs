namespace TechMove.Web.Services
{
    // Handles file upload validation and saving for signed contract agreements
    public class FileValidationService
    {
        private readonly IWebHostEnvironment _env;
        private const string UploadFolder = "uploads/contracts";
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB limit (Code, 2025)

        public FileValidationService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Only PDF files are accepted at a max 10 MB (Code, 2025)
        public void Validate(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file), "No file was provided.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                throw new InvalidOperationException($"Invalid file type '{extension}'. Only PDF files are accepted.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException($"File size {file.Length / (1024 * 1024):F1} MB exceeds the 10 MB limit.");
        }

        // Saves the file with a unique name to prevent overwrites, returns the relative path (Code, 2025)
        public async Task<string> SaveAsync(IFormFile file)
        {
            Validate(file);

            var uploadPath = Path.Combine(_env.WebRootPath, UploadFolder);
            Directory.CreateDirectory(uploadPath);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadPath, uniqueName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{UploadFolder}/{uniqueName}";
        }
    }
}
