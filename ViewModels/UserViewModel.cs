using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.ViewModels;

public partial class UserViewModel
{
    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }

    public string? FullName { get; set; }

    public int? PasswordResetCode { get; set; }

    public DateTime? PasswordResetCodeExpiry { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string CreatedUser { get; set; } = null!;

    public DateTime CreatedDttm { get; set; }

    public string? ModifiedUser { get; set; }

    public DateTime? ModifiedDttm { get; set; }


}
