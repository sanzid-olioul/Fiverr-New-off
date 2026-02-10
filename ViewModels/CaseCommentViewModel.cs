using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels
{
    public partial class CaseCommentViewModel
    {
        public int CommentId { get; set; }

        public int CaseId { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        public string CommentText { get; set; } = null!;

        public string? CreatedUser { get; set; }

        public DateTime? CreatedDttm { get; set; }

        public string? ModifiedUser { get; set; }

        public DateTime? ModifiedDttm { get; set; }

        [Required(ErrorMessage = "Record status is required")]
        public string RecordStatus { get; set; } = null!;
    }
}
