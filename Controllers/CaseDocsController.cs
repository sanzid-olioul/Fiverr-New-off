using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to manage case documents, including listing, uploading, editing, and deleting documents.
    /// </summary>
    public class CaseDocsController : BaseController
    {
        private readonly CaseDocumentsService _caseDocumentsDataAccess;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public CaseDocsController(CaseDocumentsService caseDocumentsDataAccess, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _caseDocumentsDataAccess = caseDocumentsDataAccess;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
        }

        #region List Documents Page

        /// <summary>
        /// Displays the list of documents for the current case.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListDocuments()
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (caseId == null) return RedirectToAction("Index", "Cases");

            var reqQueue = await _caseDocumentsDataAccess.GetRequestsInQueueAsync(caseId);
            ViewBag.QueueCount = reqQueue?.Count;

            var documents = await _caseDocumentsDataAccess.ListCaseDocumentsByIdAsync(caseId);
            return View(documents);
        }

        /// <summary>
        /// Retrieves AI results for a specific document.
        /// </summary>
        [HttpGet("CaseDocs/GetResultFromAI/{docId}")]
        public async Task<IActionResult> GetResultFromAI(string docId)
        {
            var responseResults = await _caseDocumentsDataAccess.GetResultFromAIAsync(docId);
            return Json(new { success = true, results = responseResults.Response, docName = responseResults.DocumentName });
        }

        /// <summary>
        /// Retrieves the current requests in the queue for a case.
        /// </summary>
        [HttpGet("CaseDocs/GetRequestsInQueue/")]
        public async Task<IActionResult> GetRequestsInQueue()
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (caseId == null) return RedirectToAction("Index", "Cases");

            var responseResults = await _caseDocumentsDataAccess.GetRequestsInQueueAsync(caseId);
            return Json(new { success = true, results = responseResults });
        }

        /// <summary>
        /// Downloads a specific document by ID.
        /// </summary>
        [HttpGet("CaseDocs/DownloadDocument/{documentId}")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            return await _caseDocumentsDataAccess.DownloadDocumentAsync(documentId);
        }

        /// <summary>
        /// Initiates an eligibility check for a list of documents.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckEligibility([FromBody] List<CaseDocumentViewModel> documents)
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (caseId == null) return RedirectToAction("Index", "Cases");

            await _caseDocumentsDataAccess.AddDocumentToQueue((int)Convert.ToDecimal(caseId), documents);

            return Json(new { success = true, message = "Your eligibility check has been initiated. You will receive an email with the results shortly." });
        }

        #endregion

        #region Edit Document

        /// <summary>
        /// Returns the view to edit a specific document by ID.
        /// </summary>
        [HttpGet("CaseDocs/EditDocument/{documentId}")]
        public async Task<IActionResult> EditDocument(int documentId)
        {
            var document = await _caseDocumentsDataAccess.GetCaseDocumentByIdAsync(documentId);
            if (document == null) return NotFound();

            ViewBag.RecordStatusData = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "A" },
                new SelectListItem { Text = "Deleted", Value = "D" }
            };

            ViewBag.DocTypeData = await _commonService.GetDomainSelectListAsync("DOC_TYPE", document.DocType);
            return View(document);
        }

        /// <summary>
        /// Updates an existing document with the provided details.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocument(CaseDocumentViewModel model, IFormFile documentUpload)
        {
            ViewBag.RecordStatusData = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "A" },
                new SelectListItem { Text = "Deleted", Value = "D" }
            };

            ViewBag.DocTypeData = await _commonService.GetDomainSelectListAsync("DOC_TYPE", model.DocType);

            var result = await _caseDocumentsDataAccess.EditDocumentAsync(model, documentUpload);
            if (result)
            {
                _commonService.SetTempData("Successfully edited the document", "success");
                return RedirectToAction(nameof(ListDocuments));
            }

            _commonService.SetTempData("Failed to edit the document", "error");
            return View(model);
        }

        #endregion

        #region Case Document Deletion

        /// <summary>
        /// Deletes a specific document by ID.
        /// </summary>
        [HttpPost("CaseDocs/DeleteConfirmed/{documentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int documentId)
        {
            var isDeleted = await _caseDocumentsDataAccess.DeleteCaseDocumentAsync(new CaseDocumentViewModel { DocId = documentId });
            if (isDeleted)
            {
                _commonService.SetTempData("Document deleted successfully.", "success");
            }
            else
            {
                _commonService.SetTempData("Failed to delete the document.", "error");
            }

            return RedirectToAction(nameof(ListDocuments));
        }

        #endregion

        #region View Document

        /// <summary>
        /// Returns whether the document is a PDF (used by client to allow viewing).
        /// </summary>
        [HttpGet("CaseDocs/IsPdf/{documentId}")]
        public async Task<IActionResult> IsPdf(int documentId)
        {
            var isPdf = await _caseDocumentsDataAccess.IsPdfAsync(documentId);
            return Json(new { isPdf });
        }

        /// <summary>
        /// Streams the PDF document inline for viewing in the browser/pdf viewer.
        /// Only allows PDF files; non-PDFs return BadRequest.
        /// </summary>
        [HttpGet("CaseDocs/ViewDocument/{documentId}")]
        public async Task<IActionResult> ViewDocument(int documentId)
        {
            var document = await _caseDocumentsDataAccess.GetCaseDocumentByIdAsync(documentId);
            if (document == null) return NotFound();

            var extension = System.IO.Path.GetExtension(document.Name)?.ToLower();
            if (extension != ".pdf")
            {
                return BadRequest("Only PDF files can be viewed.");
            }

            return File(document.Content!, "application/pdf");
        }

        #endregion

        #region Upload Document

        /// <summary>
        /// Returns the view to upload a new document.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UploadDocument()
        {
            var model = new CaseDocumentViewModel();
            ViewBag.DocTypeData = await _commonService.GetDomainSelectListAsync("DOC_TYPE");
            return View(model);
        }

        /// <summary>
        /// Uploads a new document for the current case.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument([Bind("Name,DocType,DocumentUpload")] CaseDocumentViewModel caseDocumentViewModel)
        {
            ViewBag.DocTypeData = await _commonService.GetDomainSelectListAsync("DOC_TYPE");

            var caseId = _sessionMergeService.GetCurrentCaseId();
            var success = await _caseDocumentsDataAccess.UploadDocumentAsync(caseDocumentViewModel, caseId);

            if (!success)
            {
                _commonService.SetTempData("Failed to upload document", "error");
                return RedirectToAction(nameof(ListDocuments));
            }

            _commonService.SetTempData("Successfully uploaded the document", "success");
            return RedirectToAction(nameof(ListDocuments));
        }

        #endregion

        #region View PDF (No wwwroot, No Temp Files)

        /// <summary>
        /// Loads PDF viewer page
        /// </summary>
        [HttpGet("CaseDocs/ViewPdf/{documentId}")]
        public IActionResult ViewPdf(int documentId)
        {
            ViewBag.DocumentId = documentId;
            return View();
        }

        /// <summary>
        /// Streams PDF directly from DB for Syncfusion viewer
        /// </summary>
        [HttpGet("CaseDocs/StreamPdf/{documentId}")]
        public async Task<IActionResult> StreamPdf(int documentId)
        {
            var document = await _caseDocumentsDataAccess.GetCaseDocumentByIdAsync(documentId);
            if (document == null)
                return NotFound();

            var extension = Path.GetExtension(document.Name)?.ToLower();
            if (extension != ".pdf")
                return BadRequest("Only PDF files are supported.");

            return File(document.Content!, "application/pdf");
        }

        #endregion

    }
}
