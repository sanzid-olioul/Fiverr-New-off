using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;

namespace LancasterCreditCardDiversion.Controllers
{

    /// Simple public document listing and upload controller to help test the PDF viewer without login.
    /// Note: Upload endpoint is intended for local/dev testing only
    
    public class PublicDocsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public PublicDocsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Lists files in wwwroot/docs and shows simple upload form.
        /// </summary>
        public IActionResult Index()
        {
            var docsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "docs");
            if (!Directory.Exists(docsFolder)) Directory.CreateDirectory(docsFolder);

            var files = Directory.EnumerateFiles(docsFolder)
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList();

            return View(files);
        }

        /// <summary>
        /// Uploads a PDF to wwwroot/docs (dev-only).
        /// </summary>
        [HttpPost]
        public IActionResult UploadFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "No file selected.";
                return RedirectToAction("Index");
            }

            var docsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "docs");
            if (!Directory.Exists(docsFolder)) Directory.CreateDirectory(docsFolder);

            // Only allow pdfs
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".pdf")
            {
                TempData["Message"] = "Only PDF files are allowed for this demo.";
                return RedirectToAction("Index");
            }

            var safeName = Path.GetFileName(file.FileName);
            var path = Path.Combine(docsFolder, safeName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            TempData["Message"] = "File uploaded.";
            return RedirectToAction("Index");
        }
    }
}
