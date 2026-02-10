using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class LetterTemplate
{
    public int LetterTemplateId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime PublishedDate { get; set; }

    public byte[] Content { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public string ConvertToPdf { get; set; } = null!;

    public string DocType { get; set; } = null!;

    public string DocTypeDomainName { get; set; } = null!;

    public Guid Rowid { get; set; }

    public virtual AppDomainValue AppDomainValue { get; set; } = null!;
}
