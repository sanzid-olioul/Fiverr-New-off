namespace LancasterCreditCardDiversion.ViewModels
{
    public class ResponsesApiRequestsViewModel
    {
        public decimal CaseId { get; set; }

        public string? PromptId { get; set; }

        public string? PromptVersion { get; set; }

        public string? Response { get; set; }

        public string EligibilityCheckStatusDomainName { get; set; } = null!;

        public string EligibilityCheckStatus { get; set; } = null!;

        public string RecordStatus { get; set; } = null!;

        public string CreatedUser { get; set; } = null!;

        public DateTime CreatedDttm { get; set; }

        public string ModifiedUser { get; set; } = null!;

        public DateTime ModifiedDttm { get; set; }

        public decimal VersionId { get; set; }

        public decimal ReqId { get; set; }

        public bool? IsInProgress { get; set; }

        public bool? IsChecked { get; set; }

        public string? DocumentNames { get; set; }
    }
}
