using LancasterCreditCardDiversion.Services;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to handle operations related to case comments, including listing, adding, and deleting comments.
    /// </summary>
    public class EligibilityCheckRequestsController : BaseController
    {
        private readonly EligibilityCheckRequestsService _eligibilityCheckRequestsService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public EligibilityCheckRequestsController(EligibilityCheckRequestsService eligibilityCheckRequestsService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _eligibilityCheckRequestsService = eligibilityCheckRequestsService;
            _sessionMergeService = sessionMergeService;
        }

        /// <summary>
        /// Displays a list of requests for the current case.
        /// </summary>
        public async Task<IActionResult> ListRequests()
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (caseId == null) return RedirectToAction("Index", "Cases");

            var reqQueue = await _eligibilityCheckRequestsService.GetRequestsInQueueAsync(caseId);
            if (reqQueue != null)
            {
                ViewBag.QueueCount = reqQueue.Count;
            }

            //var requests = await _eligibilityCheckRequestsService.GetRequestsByIdAsync(caseId);
            var requests = await _eligibilityCheckRequestsService.GetResponsesAPIByCaseIdAsync(caseId);
            return View(requests);
        }


        //[HttpGet("EligibilityCheckRequests/AIResponse/{threadId}")]
        //public async Task<IActionResult> AIResponse(string threadId)
        //{
        //    var responseResults = await _eligibilityCheckRequestsService.GetAIResponseByIdAsync(threadId);
        //    return Json(new { success = true, results = responseResults.Response, docNames = responseResults.DocumentNames});
        //}

        [HttpGet("EligibilityCheckRequests/AIResponse/{caseId}/{reqId}")]
        public async Task<IActionResult> GetResponseById(decimal caseId, decimal reqId)
        {
            var result = await _eligibilityCheckRequestsService.GetResponseAPIByIdAsync(caseId, reqId);
            if (result == null)
            {
                return NotFound(new { success = false, message = "Response not found." });
            }

            var (response, documentNames) = result.Value;

            return Ok(new
            {
                success = true,
                response = response.Response,
                documentNames,
                caseId = response.CaseId,
                reqId = response.ReqId
            });
        }

    }
}