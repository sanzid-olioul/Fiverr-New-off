using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class EligibilityCheckRequestDocument
{
    public int CheckRequestDocId { get; set; }

    public int ReqId { get; set; }

    public int DocId { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual CaseDocument Doc { get; set; } = null!;

    public virtual ResponsesApiRequest Req { get; set; } = null!;
}
