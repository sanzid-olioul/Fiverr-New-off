using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels;

public class CaseHistoryHearingDateViewModel
{
    public int CaseId { get; set; }
/*    public string? CaseStatus { get; set; }*/
    public int? CurrentHearingId { get; set; }
    public DateTime? PreviousHearingId { get; set; }
}

