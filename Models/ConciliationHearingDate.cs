using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class ConciliationHearingDate
{
    public int HearingId { get; set; }

    public DateTime HearingDttm { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual ICollection<CcdpCase> CcdpCases { get; set; } = new List<CcdpCase>();
}
