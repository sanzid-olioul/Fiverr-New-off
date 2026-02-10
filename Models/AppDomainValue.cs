using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class AppDomainValue
{
    public string DomainName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual ICollection<CaseDocument> CaseDocuments { get; set; } = new List<CaseDocument>();

    public virtual ICollection<CaseHistory> CaseHistories { get; set; } = new List<CaseHistory>();

    public virtual ICollection<CcdpCase> CcdpCases { get; set; } = new List<CcdpCase>();

    public virtual AppDomain DomainNameNavigation { get; set; } = null!;

    public virtual ICollection<EligibilityCheckRequest> EligibilityCheckRequests { get; set; } = new List<EligibilityCheckRequest>();

    public virtual ICollection<LetterTemplate> LetterTemplates { get; set; } = new List<LetterTemplate>();

    public virtual ICollection<ResponsesApiRequest> ResponsesApiRequests { get; set; } = new List<ResponsesApiRequest>();
}
