using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class CaseHistory
{
    public int CaseHistoryId { get; set; }

    public int CaseId { get; set; }

    public string EventType { get; set; } = null!;

    public string EventTypeDomainName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public string Description { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;

    public virtual CcdpCase Case { get; set; } = null!;
}
