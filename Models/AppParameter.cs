using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class AppParameter
{
    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Description { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string ModifiedUser { get; set; } = null!;

    public DateTime ModifiedDttm { get; set; }

    public double VersionId { get; set; }

    public Guid Rowid { get; set; }

    public int ParameterId { get; set; }
}
