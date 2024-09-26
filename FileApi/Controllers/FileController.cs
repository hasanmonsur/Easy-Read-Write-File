using Microsoft.AspNetCore.Mvc;

namespace FileApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public FileController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file selected.");
                }

                var permittedExtensions = new[] { ".txt", ".jpg", ".png" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    return BadRequest("Unsupported file type.");
                }

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            

            var path = Path.Combine(_environment.ContentRootPath, "uploads", uniqueFileName);
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, $"I/O error occurred: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException authEx)
            {
                return StatusCode(403, $"Permission error: {authEx.Message}");
            }

            return Ok(new { file_name = uniqueFileName });
        }


        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            var path = Path.Combine(_environment.ContentRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
