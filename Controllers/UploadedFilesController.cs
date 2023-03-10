using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODrive.Models;

namespace ODrive.Controllers
{
    [ApiController]
    public class UploadedFilesController : ControllerBase
    {
        private readonly ILogger _logger = LoggerFactory.Create(builder =>
            builder.AddConsole()).CreateLogger<UploadedFilesController>();
        private readonly ODriveContext _context;

        public UploadedFilesController(ODriveContext context)
        {
            _context = context;
        }

        // /health endpoint
        // Return the health of the service as HTTP 200 status.
        // Useful to check if everything is configured correctly.
        [HttpGet("/health")]
        public ActionResult<string> GetHealth()
        {
            _logger.LogInformation("GET: /health endpoint called.");
            return Ok();
        }

        // GET: /files/5
        [HttpGet("/files/{id}")]
        public async Task<ActionResult<UploadedFile>> GetUploadedFile(string id)
        {
            // Escape the curly braces in the log message.
            _logger.LogInformation("GET: /files/{{id}} endpoint called.");
            
            if (_context.UploadedFiles == null)
            {
                _logger.LogWarning("Database is not initialized.");
                return NotFound();
            }

            _logger.LogInformation("Getting file with id: " + id);
            var uploadedFile = await _context.UploadedFiles.FindAsync(id);

            if (uploadedFile == null || uploadedFile.Data == null)
            {
                return NotFound();
            }

            // Return the file, with the appropriate content type and file name.
            return File(uploadedFile.Data, uploadedFile.ContentType, uploadedFile.Name);

        }

        // GET: /files
        [HttpGet("/files")]
        public async Task<ActionResult<IEnumerable<UploadedFile>>> GetUploadedFiles()
        {
            _logger.LogInformation("GET: /files endpoint called.");
            if (_context.UploadedFiles == null)
            {
                // Not in original schema, but this is the appropriate response.
                _logger.LogWarning("Database is not initialized.");
                return NotFound();
            }
            // Remove the data and type from the response as they would be displayed
            // as text in the array response which is not compliant with the schema.
            return await _context.UploadedFiles.Select(f => new UploadedFile
            {
                Fileid = f.Fileid,
                Name = f.Name,
                Size = f.Size,
                Created_at = f.Created_at
            })
                .ToListAsync();
        }

        // POST: /files
        [HttpPost("/files")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UploadedFile>> PostUploadedFile([FromForm] IFormFile data)
        {
            _logger.LogInformation("POST: /files endpoint called.");
            if (_context.UploadedFiles == null)
            {
                // Not in original schema but including for consistency with the above.
                _logger.LogWarning("Database is not initialized.");
                return NotFound("Database is not initialized.");
            }

            if (data == null || data.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // For now, checking the suffix of the file as a proxy for the content type.
            string fileType = data.FileName.Substring(data.FileName.LastIndexOf('.') + 1);
            
            // Coalesce .mpg to .mpeg.
            fileType = fileType == "mpg" ? "mpeg" : fileType;
            if (fileType != "mpeg" && fileType != "mp4")
            {
                return new UnsupportedMediaTypeResult();
            }

            UploadedFile queuedFile = new UploadedFile{
                Fileid = Guid.NewGuid().ToString(),
                Created_at = DateTime.Now,
                Name = data.FileName,
                ContentType = "video/" + fileType
            };
            
            byte[] dataArray;
            using (var br = new BinaryReader(data.OpenReadStream()))
            {
                dataArray = br.ReadBytes((int)data.Length);
            }

            queuedFile.Data = dataArray;
            queuedFile.Size = (int)data.Length;

            _context.UploadedFiles.Add(queuedFile);

            try
            {
                _logger.LogInformation("Saving file to database.");
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UploadedFileExists(data.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUploadedFile", new { id = queuedFile.Fileid }, null);
        }

        // PUT: /files/5
        // This allows for updating the file name.
        [HttpPut("/files/{id}")]
        public async Task<IActionResult> PutUploadedFile(string id, string newName)
        {
            _logger.LogInformation("PUT: /files/{{id}} endpoint called.");

            if (_context.UploadedFiles == null)
            {
                _logger.LogWarning("Database is not initialized.");
                return NotFound();
            }

            _context.UploadedFiles.Find(id).Name = newName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UploadedFileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUploadedFile", new { id = id }, null);
        }

        // DELETE: /files/5
        [HttpDelete("/files/{id}")]
        public async Task<IActionResult> DeleteUploadedFile(string id)
        {
            _logger.LogInformation("DELETE: /files/{{id}} endpoint called.");
            
            if (_context.UploadedFiles == null)
            {
                return NotFound();
            }
            var uploadedFile = await _context.UploadedFiles.FindAsync(id);
            if (uploadedFile == null)
            {
                return NotFound();
            }

            _context.UploadedFiles.Remove(uploadedFile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UploadedFileExists(string name)
        {
            return (_context.UploadedFiles?.Any(e => e.Name == name)).GetValueOrDefault();
        }
    }
}
