using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LancasterCreditCardDiversion.ViewModels;

public partial class ConciliationHearingDateViewModel
{
    public int HearingId { get; set; }

    public DateTime HearingDttm { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDttm { get; set; }

    public string? ModifiedUser { get; set; }

    public DateTime? ModifiedDttm { get; set; }

    [Required(ErrorMessage = "Record status is required")]
    public string RecordStatus { get; set; } = null!;

    public List<DateTime> SelectedDates { get; set; } = new List<DateTime>();
}
