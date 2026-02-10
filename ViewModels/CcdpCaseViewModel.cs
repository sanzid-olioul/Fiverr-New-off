using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels;

public class CcdpCaseViewModel
{
    public int CaseId { get; set; }

    [Required(ErrorMessage = "Court Case Number is required")]
    public string CourtCaseNumber { get; set; } = null!;

    [Required(ErrorMessage = "Filing Date is required")]
    public DateTime FilingDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Plaintiff Name is required")]
    public string PlaintiffName { get; set; } = null!;

    [Required(ErrorMessage = "Plaintiff Rep is required")]
    public string? PlaintiffRepName { get; set; }

    public string? PlaintiffRep2Name { get; set; }

    [Required(ErrorMessage = "Defendant Name is required")]
    public string DefendantName { get; set; } = null!;

    public string? DefendantTwoName { get; set; }
   
    public string? DefendantRepName { get; set; }

    public string? DefendantRep2Name { get; set; }

    [Required(ErrorMessage = "Case Status is required")]
    public string CaseStatus { get; set; } = "N";

    public int? HearingId { get; set; }

    public DateTime? HearingDttm { get; set; }

    public string? CaseComment { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDttm { get; set; }

    public string? ModifiedUser { get; set; }

    public DateTime? ModifiedDttm { get; set; }

    public string RecordStatus { get; set; } = "A";

    public IFormFile[]? AdditionalDocuments { get; set; }

    public bool isEligibilityChecked { get; set; } = false;
    public string? FilingDateSearchValue { get; set; }
    public string? HearingDateRange {  get; set; }
    public string? CaseStatusCode { get; set; }
    public string? RecordStatusCode { get; set; }
    public string? PlaintiffRepLawfirmName { get; set; }
    public string? DefendantRepLawfirmName { get; set; }
    public string PreviousHearingDates { get; set; } = "-";

}
