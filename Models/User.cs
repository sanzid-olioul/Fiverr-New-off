using System;
using System.Collections.Generic;

namespace LancasterCreditCardDiversion.Models;

public partial class User
{
    public string? UserName { get; set; }

    public string? PasswordHash { get; set; }

    public string? Email { get; set; }

    public string? RecordStatus { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDttm { get; set; }

    public string? ModifiedUser { get; set; }

    public DateTime? ModifiedDttm { get; set; }

    public string? FullName { get; set; }

    public int? PasswordResetCode { get; set; }

    public DateTime? PasswordResetCodeExpiry { get; set; }

    public int? VersionId { get; set; }
}
