using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class AppDomain
{
    public string DomainName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public virtual ICollection<AppDomainValue> AppDomainValues { get; set; } = new List<AppDomainValue>();
}
