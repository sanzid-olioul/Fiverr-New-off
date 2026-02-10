using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LancasterCreditCardDiversion.ViewModels;

public partial class AppDomainValueViewModel
{
    public string DomainName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;
}
