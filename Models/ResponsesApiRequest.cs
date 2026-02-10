using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class ResponsesApiRequest
{
    public int ReqId { get; set; }

    public int CaseId { get; set; }

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

    public int VersionId { get; set; }

    public bool IsInProgress { get; set; }

    public bool IsChecked { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;

    public virtual CcdpCase Case { get; set; } = null!;

    public virtual ICollection<EligibilityCheckRequestDocument> EligibilityCheckRequestDocuments { get; set; } = new List<EligibilityCheckRequestDocument>();
}
