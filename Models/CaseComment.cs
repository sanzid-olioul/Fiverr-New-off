using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class CaseComment
{
    public int CommentId { get; set; }

    public int CaseId { get; set; }

    public string CommentText { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual CcdpCase Case { get; set; } = null!;
}
