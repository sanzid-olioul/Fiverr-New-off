using LancasterCreditCardDiversion.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;

namespace LancasterCreditCardDiversion.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string? _smtpUsername;
        private readonly string? _smtpPassword;
        private readonly string? _smtpHost;

        public EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpUsername = options.Value.Username;
            _smtpPassword = options.Value.Password;
            _smtpHost = options.Value.Host;
        }

        public async Task<bool> SendEmailAsync(string emailFrom, string emailTo, string emailSubject, string emailBody)
        {
            _logger.LogInformation("SendEmail - start to: {EmailTo}, subject: {EmailSubject}", emailTo, emailSubject);
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Do Not Reply", emailFrom));
                email.To.Add(MailboxAddress.Parse(emailTo));
                email.Subject = emailSubject;
                email.Body = new TextPart(TextFormat.Text) { Text = emailBody };

                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(_smtpHost, 25, SecureSocketOptions.StartTls);
                    //await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }

                _logger.LogInformation("SendEmail - completed successfully to: {EmailTo}, subject: {EmailSubject}", emailTo, emailSubject);
                return true;
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError("Authentication failed: {ErrorMessage}", authEx.Message);
                if (authEx.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerErrorMessage}", authEx.InnerException.Message);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email: {ErrorMessage}", ex.Message);
                return false;
            }
        }
    }
}
