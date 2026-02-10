using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.Controllers
{
    public class PdfViewerController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfViewerController> _logger;

        public PdfViewerController(IWebHostEnvironment environment, ILogger<PdfViewerController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                _logger.LogWarning("PdfViewer Index called without file parameter");
                return View();
            }

            _logger.LogInformation("Loading PDF viewer for file: {FileName}", file);
            ViewBag.File = file;
            return View();
        }

        [HttpGet]
        public IActionResult GetPdf(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                _logger.LogWarning("GetPdf called without file parameter");
                return NotFound("No file specified");
            }

            var path = Path.Combine(_environment.WebRootPath, "docs", file);
            _logger.LogInformation("Looking for PDF at: {Path}", path);
            
            if (!System.IO.File.Exists(path))
            {
                _logger.LogError("PDF file not found at: {Path}", path);
                return NotFound($"File not found: {file}");
            }

            try
            {
                var bytes = System.IO.File.ReadAllBytes(path);
                _logger.LogInformation("Successfully loaded PDF: {FileName}, Size: {Size} bytes", file, bytes.Length);
                Response.Headers.Append("Content-Disposition", "inline");
                Response.Headers.Append("Access-Control-Allow-Origin", "*");
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading PDF file: {FileName}", file);
                return StatusCode(500, "Error reading file");
            }
        }
    }
}
