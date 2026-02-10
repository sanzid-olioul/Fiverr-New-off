using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles business logic and data access for case comments, including listing, adding, and deleting comments.
    /// </summary>
    public class CaseCommentsService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;

        public CaseCommentsService(PaLancCcdpDevDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
        }

        #region Case Comment Operations

        /// <summary>
        /// Retrieves a list of comments for a given case ID.
        /// </summary>
        public async Task<List<CaseCommentViewModel>> GetCommentsByIdAsync(string caseId)
        {
            if (!decimal.TryParse(caseId, out var id)) return new List<CaseCommentViewModel>();

            var commentList = await _context.CaseComments
                .Where(c => c.CaseId == id)
                .OrderByDescending(cc => cc.CreatedDttm)
                .ToListAsync();

            return commentList.Select(c => new CaseCommentViewModel
            {
                CommentId = c.CommentId,
                CommentText = c.CommentText,
                CreatedDttm = c.CreatedDttm,
                CreatedUser = c.CreatedUser,
                RecordStatus = c.RecordStatus == "A" ? "Active" : "Deleted"
            }).ToList();
        }

        /// <summary>
        /// Adds a new comment to the specified case.
        /// </summary>
        public async Task<bool> AddCommentAsync(CaseCommentViewModel commentToAdd, string caseId)
        {
            if (!decimal.TryParse(caseId, out var caseIdDecimal)) return false;

            var newComment = new CaseComment
            {
                CaseId = (int)caseIdDecimal,
                CommentText = commentToAdd.CommentText,
                RecordStatus = "A",
                CreatedUser = _sessionUser ?? "Unknown",
                ModifiedUser = _sessionUser ?? "Unknown"
            };

            await _context.CaseComments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Marks a comment as deleted by setting its RecordStatus.
        /// </summary>
        public async Task<bool> DeleteCommentAsync(CaseCommentViewModel commentToDelete)
        {
            var existingComment = await _context.CaseComments.FindAsync(commentToDelete.CommentId);
            if (existingComment == null) return false;

            existingComment.RecordStatus = "D";
            existingComment.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}