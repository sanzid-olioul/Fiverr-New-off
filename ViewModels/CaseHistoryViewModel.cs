using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.ViewModels;

public partial class CaseHistoryViewModel
{
    public int CaseHistoryId { get; set; }

    public int CaseId { get; set; }

    public string EventType { get; set; } = null!;

    public string EventTypeDomainName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public string Description { get; set; } = null!;
    public string RecordStatus { get; set; } = null!;

}
