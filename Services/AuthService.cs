using LancasterCreditCardDiversion.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using LancasterCreditCardDiversion.Models;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Provides authentication and user-related services such as password reset, username retrieval, and authentication.
    /// </summary>
    public class AuthService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly AuthDbContext _authcontext;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Enumeration for the result of password reset code verification.
        /// </summary>
        public enum ResetCodeVerificationResult
        {
            Valid,
            Expired,
            Invalid
        }

        public AuthService(PaLancCcdpDevDbContext context, EmailService emailService, ILogger<AuthService> logger, AuthDbContext authcontext, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _authcontext = authcontext;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user by username and password.
        /// </summary>
        /// <param name="username">The username of the user to authenticate.</param>
        /// <param name="password">The password of the user to authenticate.</param>
        /// <returns>True if authentication is successful; otherwise, false.</returns>
        public bool AuthenticateUser(string username, string password)
        {
            string? schemaName = _configuration["DatabaseSettings:SchemaName"];
            string sql = $"SELECT {schemaName}.AuthenticateUser(@username, @password) AS Status";
            var usernameParam = new SqlParameter("@username", SqlDbType.VarChar) { Value = username };
            var passwordParam = new SqlParameter("@password", SqlDbType.VarChar) { Value = password };

            var status = _authcontext.AuthenticateUserResults
                .FromSqlRaw(sql, usernameParam, passwordParam)
                .AsEnumerable()
                .FirstOrDefault();

            if (status == null)
            {
                _logger?.LogError("Authentication query returned null for username: {Username}", username);
                return false;
            }

            return status.Status == "TRUE";
            
        }

        /// <summary>
        /// Retrieves the full name of a user by their username.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The full name of the user if found; otherwise, an empty string.</returns>
        public string GetUserFullName(string username)
        {
            var allUpperCaseUsername = username.ToUpper();
            var fullName = _context.Users.Where(u => allUpperCaseUsername == u.UserName).Select(u => u.FullName).FirstOrDefault();

            return fullName ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the username associated with a given email.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The username if found; otherwise, an empty string.</returns>
        public string GetUsernameFromEmail(string email)
        {
            var allUpperCaseEmail = email.ToUpper();
            var username = _context.Users.Where(u => allUpperCaseEmail.Equals(u.Email, StringComparison.CurrentCultureIgnoreCase)).Select(u => u.UserName).FirstOrDefault();

            return username ?? string.Empty;
        }

        /// <summary>
        /// Initiates a password reset process by sending a reset code to the user's email.
        /// </summary>
        /// <param name="username">The username of the user (optional).</param>
        /// <param name="email">The email of the user (optional).</param>
        /// <returns>True if the reset code was successfully sent; otherwise, false.</returns>
        public async Task<bool> ResetPassword(string? username, string? email)
        {
            string emailFrom = _configuration["EmailSettings:EmailFrom"]!;
            var user = await _context.Users.FirstOrDefaultAsync(u => (username != null && u.UserName == username.ToUpper()) || u.Email == email);

            if (user != null && user.RecordStatus == "A" && !string.IsNullOrEmpty(user.Email))
            {
                int resetCode = new Random().Next(100000, 999999);
                user.PasswordResetCode = resetCode;
                user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(30);

                await _context.SaveChangesAsync();

                string emailSubject = "Password Reset Code";
                string emailBody = $"Your password reset code is: {resetCode}";

                return await _emailService.SendEmailAsync(emailFrom, user.Email, emailSubject, emailBody);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies a reset code for password reset.
        /// </summary>
        /// <param name="resetCode">The reset code to verify.</param>
        /// <param name="usernameOrEmail">The username or email of the user.</param>
        /// <returns>A ResetCodeVerificationResult indicating whether the reset code is valid, expired, or invalid.</returns>
        public async Task<ResetCodeVerificationResult> VerifyResetCodeAsync(string resetCode, string usernameOrEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => (usernameOrEmail != null && u.UserName == usernameOrEmail.ToUpper()) || u.Email == usernameOrEmail);

            if (user == null)
            {
                return ResetCodeVerificationResult.Invalid;
            }

            if (user.PasswordResetCode != Convert.ToInt32(resetCode))
            {
                return ResetCodeVerificationResult.Invalid;
            }

            if (user.PasswordResetCodeExpiry <= DateTime.UtcNow)
            {
                return ResetCodeVerificationResult.Expired;
            }
            return ResetCodeVerificationResult.Valid;
        }

        /// <summary>
        /// Sets a new password for a user.
        /// </summary>
        /// <param name="newPass">The new password to set.</param>
        /// <param name="usernameOrEmail">The username or email of the user.</param>
        /// <returns>True if the password is successfully updated; otherwise, false.</returns>
        public bool SetNewPasswordAsync(string newPass, string usernameOrEmail)
        {
            string? schemaName = _configuration["DatabaseSettings:SchemaName"];
            string sql = $"EXEC {schemaName}.ResetPassword @Username = @Username, @NewPassword = @NewPassword, @Result = @Result OUTPUT;";

            var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };

            _authcontext.Database.ExecuteSqlRaw(
                sql,
                new SqlParameter("@Username", SqlDbType.VarChar) { Value = usernameOrEmail },
                new SqlParameter("@NewPassword", SqlDbType.VarChar) { Value = newPass },
                resultParam
            );

            return resultParam.Value?.ToString() == "TRUE";
            
        }
    }
}
