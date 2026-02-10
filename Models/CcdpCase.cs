using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class CcdpCase
{
    public int CaseId { get; set; }

    public string CourtCaseNumber { get; set; } = null!;

    public DateTime FilingDate { get; set; }

    public string PlaintiffName { get; set; } = null!;

    public string? PlaintiffRepName { get; set; }

    public string? PlaintiffRep2Name { get; set; }

    public string DefendantName { get; set; } = null!;

    public string CaseStatus { get; set; } = null!;

    public string CaseStatusDomainName { get; set; } = null!;

    public int? HearingId { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public string? DefendantRepName { get; set; }

    public string? DefendantRep2Name { get; set; }

    public string? PlaintiffRepLawfirmName { get; set; }

    public string? DefendantRepLawfirmName { get; set; }

    public Guid Rowid { get; set; }

    public string? DefendantTwoName { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;

    public virtual ICollection<CaseComment> CaseComments { get; set; } = new List<CaseComment>();

    public virtual ICollection<CaseDocument> CaseDocuments { get; set; } = new List<CaseDocument>();

    public virtual ICollection<CaseHistory> CaseHistories { get; set; } = new List<CaseHistory>();

    public virtual ICollection<EligibilityCheckRequest> EligibilityCheckRequests { get; set; } = new List<EligibilityCheckRequest>();

    public virtual ConciliationHearingDate? Hearing { get; set; }

    public virtual ICollection<ResponsesApiRequest> ResponsesApiRequests { get; set; } = new List<ResponsesApiRequest>();
}
