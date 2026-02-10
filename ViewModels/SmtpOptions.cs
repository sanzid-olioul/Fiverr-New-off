using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels
{
    public class SmtpOptions
    {
        public const string ConfigSection = "Smtp";

        [Required]
        public string? Username { get; set; } = null!;

        [Required]
        [ConfigurationKeyName("Password")]
        public string? Password { get; set; } = null!;
        [Required]
        public string? Host { get; set; } = null!;
    }
}
