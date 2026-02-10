using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to manage cases, including creation, editing, deletion, and listing operations.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CasesController : BaseController
    {
        private readonly CaseService _casesService;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;
        private readonly CaseStatusClass _caseStatusClass;

        public CasesController(CaseService casesService, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService, CaseStatusClass caseStatusClass)
        {
            _casesService = casesService;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
            _caseStatusClass = caseStatusClass;
        }

        #region Case Listing Operations

        /// <summary>
        /// Displays a list of active cases.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            var cases = await _casesService.ListConditionalCasesAsync(isActiveCase: true);
            return View(cases);
        }

        /// <summary>
        /// Displays a list of all cases to search.
        /// </summary>
        public async Task<IActionResult> AllCasesSearch()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            var caseStatusData = await _commonService.GetDomainSelectListAsync("CASE_STATUS");
            ViewBag.CaseStatusData = caseStatusData
                .Select(x => new SelectListItem
                {
                    Value = x.Value,
                    Text = x.Text.ToUpper() // Convert text to uppercase
                })
                .ToList();
            ViewBag.HearingDatesRangeData = await _commonService.GetHearingDateRangesAsync();
            return View();
        }

        /// <summary>
        /// Searches for cases based on the provided criteria.
        /// </summary>
        [HttpPost("Cases/SearchCases")]
        public async Task<IActionResult> SearchCases([FromBody] CcdpCaseViewModel criteria)
        {
            var result = await _casesService.SearchCasesAsync(criteria);
            return Json(new { success = true, results = result });
        }

        #endregion

        #region Case Creation

        /// <summary>
        /// Returns the view to create a new case.
        /// </summary>
        public async Task<IActionResult> CreateCase()
        {
            ViewBag.FirstLoad = true;
            var caseStatusData = await _commonService.GetDomainSelectListAsync("CASE_STATUS");
            ViewBag.CaseStatusData = caseStatusData
                .Select(x => new SelectListItem
                {
                    Value = x.Value,
                    Text = x.Text.ToUpper() // Convert text to uppercase
                })
                .ToList();
            return View(new CcdpCaseViewModel());
        }

        /// <summary>
        /// Creates a new case.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCase(CcdpCaseViewModel ccdpcase)
        {
            ViewBag.FirstLoad = false;
            var caseStatusData = await _commonService.GetDomainSelectListAsync("CASE_STATUS");
            ViewBag.CaseStatusData = caseStatusData
                .Select(x => new SelectListItem
                {
                    Value = x.Value,
                    Text = x.Text.ToUpper() // Convert text to uppercase
                })
                .ToList();
            ViewBag.HearingDatesData = await _commonService.GetHearingDatesAfterSetDaysAsync(ccdpcase.FilingDate);

            if (!ModelState.IsValid) return View(ccdpcase);

            var (caseId, isCreated) = await _casesService.CreateCaseAsync(ccdpcase);
            if (isCreated)
            {
                _commonService.SetTempData("Successfully created the case", "success");
                return RedirectToAction("ViewCase", new { id = caseId });
            }

            _commonService.SetTempData("Failed to create the case, it may already exist", "error");
            return View(ccdpcase);
        }

        #endregion

        #region Case Edit Operations

        /// <summary>
        /// Returns the view for a specific case.
        /// </summary>
        [HttpGet("Cases/ViewCase/{CaseId}")]
        public async Task<IActionResult> ViewCase(string? caseId)
        {
            if (string.IsNullOrEmpty(caseId)) return NotFound();

            var ccdpcase = await _casesService.GetCaseByIdAsync(caseId);
            if (ccdpcase == null) return NotFound();

            var caseDocuments = await _casesService.GetCaseDocsByIdAsync(caseId);
            var caseStatusList = await _commonService.GetDomainSelectListAsync("CASE_STATUS",ccdpcase.CaseStatus.ToString());
         
            var caseStatusItem = caseStatusList.Find(x => x.Value == ccdpcase.CaseStatus.ToString().ToUpper());
            ViewBag.CaseStatusValue = new SelectListItem { Value = ccdpcase.CaseStatus.ToString(), Text = caseStatusItem?.Text };
            ViewBag.HearingIdValue = new SelectListItem { Value = ccdpcase.HearingId.ToString(), Text = ccdpcase.HearingDttm };

            await _sessionMergeService.SetCaseSessionData(ccdpcase, caseDocuments);

            return View(ccdpcase);
        }



        /// <summary>
        /// Case Session set Redirect
        /// </summary>
        [HttpGet("Cases/SetCase/{caseId}")]
        public async Task<IActionResult> SetCase(string caseId)
        {
            if (string.IsNullOrEmpty(caseId)) return NotFound();

            var ccdpcase = await _casesService.GetCaseByIdAsync(caseId);
            if (ccdpcase == null) return NotFound();

            var caseDocuments = await _casesService.GetCaseDocsByIdAsync(caseId);
            await _sessionMergeService.SetCaseSessionData(ccdpcase, caseDocuments);

            return RedirectToAction("ListDocuments", "CaseDocs");
        }

        /// <summary>
        /// Returns the view for editing a case based on the provided ID.
        /// </summary>
        [HttpGet("Cases/EditCase/{CaseId}")]
        public async Task<IActionResult> EditCase(string? caseId)
        {
            if (string.IsNullOrEmpty(caseId)) return NotFound();

            var ccdpcase = await _casesService.GetCaseByIdAsync(caseId);
            if (ccdpcase == null) return NotFound();

            var caseStatusData = await _commonService.GetDomainSelectListAsync("CASE_STATUS",ccdpcase.CaseStatus);
            ViewBag.CaseStatusData = caseStatusData
                .Select(x => new SelectListItem
                {
                    Value = x.Value,
                    Text = x.Text.ToUpper() // Convert text to uppercase
                })
                .ToList();
            ViewBag.CaseStatusValue = new SelectListItem { Value = ccdpcase.CaseStatus.ToString(), Text = ccdpcase.CaseStatus };
            ViewBag.HearingDatesData = await _commonService.GetAllHearingDatesAfterFilingDateListAsync(ccdpcase.FilingDate);
            ViewBag.HearingIdValue = new SelectListItem { Value = ccdpcase.HearingId.ToString(), Text = ccdpcase.HearingDttm };

            return View(ccdpcase);
        }

        /// <summary>
        /// Updates an existing case.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCase(decimal caseId, CcdpCaseViewModel ccdpcase)
        {
            if (caseId != ccdpcase.CaseId) return NotFound();

            var isUpdated = await _casesService.UpdateCaseAsync(ccdpcase);
            if (isUpdated)
            {
                _commonService.SetTempData("Successfully updated the case", "success");
                return RedirectToAction(nameof(ViewCase), new { caseId = ccdpcase.CaseId });
            }

            _commonService.SetTempData("Failed to update the case", "error");
            return View(ccdpcase);
        }

        #endregion

        #region Case Deletion

        /// <summary>
        /// Deletes a case based on the provided ID.
        /// </summary>
        [HttpPost("Cases/DeleteConfirmed/{caseId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal caseId)
        {
            var isDeleted = await _casesService.DeleteCaseAsync(caseId);
            if (!isDeleted)
            {
                _commonService.SetTempData("Failed to delete the case", "error");
                return NotFound();
            }

            _commonService.SetTempData("Case deleted successfully", "success");
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Hearing Date Operations

        /// <summary>
        /// Retrieves a list of hearing dates based on the filing date.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetHearingDates([FromBody] DateTime filingDateInput)
        {
            var hearingDates = await _commonService.GetHearingDatesAfterSetDaysAsync(filingDateInput);
            return Json(hearingDates);
        }

        #endregion

        #region Case Activity Log

        /// <summary>
        /// Displays the case activity log.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CaseActivityLog()
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (string.IsNullOrEmpty(caseId)) return RedirectToAction(nameof(Index));

            var caseHistories = await _casesService.GetActivityLogsByCaseIdAsync(caseId);
            return View(caseHistories);
        }

        #endregion

        #region Conciliation Management

        /// <summary>
        /// Manages conciliation details for cases.
        /// </summary>
        public async Task<IActionResult> ConciliationManagement()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            ViewBag.HearingDatesData = JsonConvert.SerializeObject(await _commonService.GetAllHearingDatesSelectListAsync());
            ViewBag.RecordStatusData = JsonConvert.SerializeObject(new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "A" },
                new SelectListItem { Text = "Deleted", Value = "D" }
            });
            var caseStatusData = await _commonService.GetDomainSelectListAsync("CASE_STATUS");

            var processedCaseStatusData = caseStatusData
                .Select(x => new SelectListItem
                {
                    Value = x.Value,
                    Text = x.Text.ToUpper() 
                })
                .ToList();

            ViewBag.CaseStatusData = JsonConvert.SerializeObject(processedCaseStatusData);
            ViewBag.CaseStatusColors = JsonConvert.SerializeObject(
                _caseStatusClass.GetCaseStatusColors().ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        kvp.Value.ClassName,
                        kvp.Value.BackgroundColor,
                        kvp.Value.TextColor
                    }
                )
            );


            var cases = await _casesService.ListConditionalCasesAsync(isActiveCase: false);
            return View(cases);
        }

        /// <summary>
        /// Updates conciliation management inline for a case.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateConciliationManagementInline([FromBody] CcdpCaseViewModel updatedCase)
        {
            if (updatedCase == null) return Json(new { success = false, message = "Invalid case data" });

            var isUpdated = await _casesService.UpdateConciliationManagementAsync(updatedCase);

            return isUpdated
                ? Json(new { success = true, message = "Case updated successfully" })
                : Json(new { success = false, message = "Failed to update case" });
        }

        /// <summary>
        /// Retrieves all comments for a specific case.
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAllCommentsByCaseId(int caseId)
        {
            var allComments = await _casesService.GetAllCommentsByCaseId(caseId);
            return Json(allComments);
        }

        #endregion

        #region Help Function

        /// <summary>
        /// Displays the help page.
        /// </summary>
        [HttpGet]
        public IActionResult Help()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            return View();
        }

        #endregion
    }
}
