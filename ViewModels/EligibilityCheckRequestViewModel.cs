using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.ViewModels
{
    public partial class EligibilityCheckRequestViewModel
    {
        public int ReqId { get; set; }

        public int CaseId { get; set; }

        public string AssistantId { get; set; } = null!;

        public string ThreadId { get; set; } = null!;

        public string? Response { get; set; }

        public string EligibilityCheckStatusDomainName { get; set; } = null!;

        public string EligibilityCheckStatus { get; set; } = null!;

        public string? RecordStatus { get; set; } 

        public string CreatedUser { get; set; } = null!;

        public DateTime CreatedDttm { get; set; }

        public string ModifiedUser { get; set; } = null!;

        public DateTime ModifiedDttm { get; set; }

        public double VersionId { get; set; }

        public bool IsChecked { get; set; } = false;

        public string? DocumentNames { get; set; }

        public bool IsInProgress { get; set; }
    }
}
