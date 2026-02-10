using Microsoft.AspNetCore.Mvc;
using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using LancasterCreditCardDiversion.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Manages letter templates, including creating, editing, merging, and downloading templates.
    /// </summary>
    public class LetterTemplatesController : BaseController
    {
        private readonly LetterTemplatesService _letterTemplatesDataAccess;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public LetterTemplatesController(LetterTemplatesService letterTemplatesDataAccess, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _letterTemplatesDataAccess = letterTemplatesDataAccess;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
        }

        /// <summary>
        /// Lists all available templates.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListTemplates()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            var templates = await _letterTemplatesDataAccess.ListTemplatesAsync();
            return View(templates);
        }

        /// <summary>
        /// Displays the page to merge templates.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MergeTemplate()
        {

            bool ifdropdown = true;
            var model = new LetterTemplateViewModel
            {
                TemplatesList = await _letterTemplatesDataAccess.ListTemplatesAsync(ifdropdown)
            };
            ifdropdown = false;
            return View(model);
        }

        /// <summary>
        /// Merges a selected template and returns a file.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MergeTemplate(LetterTemplateViewModel letterTemplateViewModel)
        {
            if (!ModelState.IsValid)
            {
                var model = new LetterTemplateViewModel
                {
                    TemplatesList = await _letterTemplatesDataAccess.ListTemplatesAsync()
                };
                return View(model);
            }

            bool convertPdf = letterTemplateViewModel.ConvertToPdf == "Y";
            var caseId = HttpContext.Session.GetString("CurrentCaseId");
            var caseDetails = _sessionMergeService.GetCaseDetailsFromSession();

            var mergedDocumentContentBytes = await _letterTemplatesDataAccess.MergeTemplateAsync(letterTemplateViewModel, caseId, caseDetails, convertPdf);

            if (mergedDocumentContentBytes.Length == 0)
            {
                _commonService.SetTempData("Duplicate file name", "error");
                return RedirectToAction(nameof(MergeTemplate));
            }

            if (convertPdf)
            {
                var content = _commonService.FlattenPdf(mergedDocumentContentBytes);
                _commonService.SetTempData("Successfully merged and uploaded PDF document.", "success");
                return File(content, "application/pdf", $"{letterTemplateViewModel.Name}.pdf");
            }

            _commonService.SetTempData("Successfully merged and uploaded Word document.", "success");
            return File(mergedDocumentContentBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{letterTemplateViewModel.Name}.docx");
        }

        /// <summary>
        /// Displays the page to create a new template.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateTemplate()
        {
            ViewBag.DocTypeData = await _letterTemplatesDataAccess.GetDocTypeSelectListAsync(null);
            return View();
        }

        /// <summary>
        /// Creates a new template with the provided details.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate(LetterTemplateViewModel letterTemplateViewModel, string plaintiffNameField, string caseDescriptionField)
        {
            ViewBag.DocTypeData = await _letterTemplatesDataAccess.GetDocTypeSelectListAsync(null);
            try
            {
                if (!await _letterTemplatesDataAccess.CreateTemplateAsync(letterTemplateViewModel, plaintiffNameField, caseDescriptionField))
                {
                    _commonService.SetTempData("Template with the same name found", "error");
                    return RedirectToAction(nameof(ListTemplates));
                }

                _commonService.SetTempData("Template created successfully", "success");
                return RedirectToAction(nameof(ListTemplates));
            }
            catch (InvalidOperationException ex)
            {
                _commonService.SetTempData(ex.Message, "error");
                return View();
            }
        }

        /// <summary>
        /// Downloads a Word template by its ID.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadWordTemplate(int id)
        {
            return await _letterTemplatesDataAccess.DownloadWordDocument(id);
        }

        /// <summary>
        /// Displays the page to edit an existing template.
        /// </summary>
        [HttpGet("LetterTemplates/EditTemplate/{templateId}")]
        public async Task<IActionResult> EditTemplate(decimal templateId)
        {
            var template = await _letterTemplatesDataAccess.GetTemplateByIdAsync(templateId);
            if (template == null) return NotFound();

            ViewBag.RecordStatusData = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "A" },
                new SelectListItem { Text = "Deleted", Value = "D" }
            };

            ViewBag.DocTypeData = await _letterTemplatesDataAccess.GetDocTypeSelectListAsync(template.DocType);
            ViewBag.DocTypeValue = new SelectListItem { Value = template.DocType.ToString(), Text = template.DocType.ToString() };
            return View(template);
        }

        /// <summary>
        /// Updates an existing template with the provided details.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTemplate(LetterTemplateViewModel model, IFormFile templateFile)
        {
            ViewBag.RecordStatusData = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "A" },
                new SelectListItem { Text = "Deleted", Value = "D" }
            };

            if (!await _letterTemplatesDataAccess.EditTemplateAsync(model, templateFile))
            {
                _commonService.SetTempData("Error while editing template", "error");
                return View(model);
            }

            _commonService.SetTempData("Template edited successfully", "success");
            return RedirectToAction(nameof(ListTemplates));
        }

        /// <summary>
        /// Deletes a template by its ID.
        /// </summary>
        [HttpPost("LetterTemplates/DeleteConfirmed/{templateId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string templateId)
        {
            if (string.IsNullOrEmpty(templateId)) return NotFound();
            if (!await _letterTemplatesDataAccess.DeleteTemplateAsync(Convert.ToDecimal(templateId)))
            {
                _commonService.SetTempData("Error deleting template", "error");
                return BadRequest();
            }

            _commonService.SetTempData("Template deleted successfully", "success");
            return RedirectToAction(nameof(ListTemplates));
        }
    }
}
