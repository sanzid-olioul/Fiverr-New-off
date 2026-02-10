using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to handle operations related to case comments, including listing, adding, and deleting comments.
    /// </summary>
    public class CaseCommentsController : BaseController
    {
        private readonly CaseCommentsService _caseCommentsService;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public CaseCommentsController(CaseCommentsService caseCommentsService, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _caseCommentsService = caseCommentsService;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
        }

        /// <summary>
        /// Displays a list of comments for the current case.
        /// </summary>
        public async Task<IActionResult> ListComments()
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (caseId == null) return RedirectToAction("Index", "Cases");

            var comments = await _caseCommentsService.GetCommentsByIdAsync(caseId);
            return View(comments);
        }

        /// <summary>
        /// Returns the view for adding a new comment.
        /// </summary>
        [HttpGet]
        public IActionResult AddComment()
        {
            var model = new CaseCommentViewModel();
            return View(model);
        }

        /// <summary>
        /// Adds a new comment to the current case.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([Bind("CommentText")] CaseCommentViewModel caseCommentViewModel)
        {
            var caseId = _sessionMergeService.GetCurrentCaseId();
            if (string.IsNullOrEmpty(caseId)) return RedirectToAction("Index", "Cases");

            var isAdded = await _caseCommentsService.AddCommentAsync(caseCommentViewModel, caseId);
            if (isAdded)
            {
                _commonService.SetTempData("Added new comment!", "success");
                return RedirectToAction(nameof(ListComments));
            }

            _commonService.SetTempData("Failed to add new comment!", "error");
            return View(caseCommentViewModel);
        }

        /// <summary>
        /// Deletes a comment based on the provided comment ID.
        /// </summary>
        [HttpPost("CaseComments/DeleteConfirmed/{commentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int commentId)
        {
            var isDeleted = await _caseCommentsService.DeleteCommentAsync(new CaseCommentViewModel { CommentId = commentId });
            if (isDeleted)
            {
                _commonService.SetTempData("Comment deleted successfully.", "success");
            }
            else
            {
                _commonService.SetTempData("Failed to delete comment.", "error");
            }

            return RedirectToAction(nameof(ListComments));
        }

        /// <summary>
        /// Retrieves comments for a case to be displayed in a modal.
        /// </summary>
        [HttpGet("CaseComments/GetCommentForModal/{caseId}")]
        public async Task<IActionResult> GetCommentForModal(string caseId)
        {
            var comments = await _caseCommentsService.GetCommentsByIdAsync(caseId);
            return Json(comments);
        }
    }
}
