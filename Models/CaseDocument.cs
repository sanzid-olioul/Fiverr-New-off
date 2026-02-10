using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class CaseDocument
{
    public int DocId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime DocDate { get; set; }

    public byte[] Content { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public string DocType { get; set; } = null!;

    public string DocTypeDomainName { get; set; } = null!;

    public int CaseId { get; set; }

    public string? TextContent { get; set; }

    public double? WordCount { get; set; }

    public Guid Rowid { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;

    public virtual CcdpCase Case { get; set; } = null!;

    public virtual ICollection<EligibilityCheckRequestDocument> EligibilityCheckRequestDocuments { get; set; } = new List<EligibilityCheckRequestDocument>();
}
