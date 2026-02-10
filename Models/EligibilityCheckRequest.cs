using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class EligibilityCheckRequest
{
    public int ReqId { get; set; }

    public int CaseId { get; set; }

    public string AssistantId { get; set; } = null!;

    public string ThreadId { get; set; } = null!;

    public string? Response { get; set; }

    public string EligibilityCheckStatusDomainName { get; set; } = null!;

    public string EligibilityCheckStatus { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public bool IsInProgress { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;

    public virtual CcdpCase Case { get; set; } = null!;
}
