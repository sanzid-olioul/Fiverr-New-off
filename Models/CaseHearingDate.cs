using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class CaseHearingDate
{
    public int CaseHearingId { get; set; }

    public double CaseId { get; set; }

    public double CaseHearingDttmId { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }
}
