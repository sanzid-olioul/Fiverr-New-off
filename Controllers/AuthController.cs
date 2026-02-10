using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Handles authentication-related operations such as login, logout, and password reset.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly CommonService _commonService;

        public AuthController(AuthService authService, CommonService commonService)
        {
            _authService = authService;
            _commonService = commonService;
        }

        /// <summary>
        /// Displays the login page
        /// </summary>
        public IActionResult Login()
        {
            TempData["Message"] = null;
            return View();
        }

        /// <summary>
        /// Authenticates the user and redirects to the dashboard if successful.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _commonService.SetTempData("Username and Password are required.", "error");
                return View();
            }

            string? username = model.UserName?.ToUpper();
            string password = model.Password;

            if (username != null)
            {
                bool isAuthenticated = _authService.AuthenticateUser(username, password);

                if (isAuthenticated)
                {
                    var sessionFullname = _authService.GetUserFullName(username);
                    HttpContext.Session.SetString("Username", username);
                    HttpContext.Session.SetString("FullName", sessionFullname);
                    _commonService.SetTempData($"Welcome, {sessionFullname}", "info");
                    return RedirectToAction("Index", "Cases");
                }

                _commonService.SetTempData("Invalid username/password", "error");
                return View();
            }

            return View();
        }

        /// <summary>
        /// Logs the user out and clears the session.
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _commonService.SetTempData("You have successfully logged out.", "success");
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Displays the Forgot Password page.
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Sends a reset password email if the username or email is valid.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(UserViewModel userViewModel)
        {
            if (userViewModel.UserName == null || userViewModel.Email == null)
            {
                _commonService.SetTempData("Please add a username or email", "error");
                return View();
            }

            var emailSentStatus = await _authService.ResetPassword(userViewModel.UserName, userViewModel.Email);

            if (emailSentStatus)
            {
                var username = userViewModel.Email != null ? _authService.GetUsernameFromEmail(userViewModel.Email) : "";
                HttpContext.Session.SetString("UsernameOrEmail", userViewModel.UserName ?? username);
                _commonService.SetTempData("Email sent successfully", "success");
                return RedirectToAction("VerifyResetCode");
            }

            _commonService.SetTempData("Resend email", "error");
            return View();
        }

        /// <summary>
        /// Displays the Verify Reset Code page.
        /// </summary>
        public IActionResult VerifyResetCode()
        {
            return View();
        }

        /// <summary>
        /// Verifies the reset code entered by the user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyResetCode(string resetCode)
        {
            if (resetCode == null)
            {
                _commonService.SetTempData("Please enter the reset code", "error");
                return View();
            }

            var usernameOrEmail = HttpContext.Session.GetString("UsernameOrEmail");

            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                _commonService.SetTempData("Session expired. Please try again.", "error");
                return RedirectToAction("ForgotPassword");
            }

            var verificationResult = await _authService.VerifyResetCodeAsync(resetCode, usernameOrEmail);

            switch (verificationResult)
            {
                case AuthService.ResetCodeVerificationResult.Valid:
                    return RedirectToAction("SetNewPassword");
                case AuthService.ResetCodeVerificationResult.Expired:
                    _commonService.SetTempData("The reset code has expired.", "error");
                    break;
                case AuthService.ResetCodeVerificationResult.Invalid:
                default:
                    _commonService.SetTempData("Invalid reset code.", "error");
                    break;
            }

            return View();
        }

        /// <summary>
        /// Displays the Set New Password page.
        /// </summary>
        public IActionResult SetNewPassword()
        {
            return View();
        }

        /// <summary>
        /// Updates the user's password.
        /// </summary>
        [HttpPost]
        public IActionResult SetNewPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                _commonService.SetTempData("Passwords do not match", "error");
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                _commonService.SetTempData("Please enter the new password", "error");
                return View();
            }

            var usernameOrEmail = HttpContext.Session.GetString("UsernameOrEmail");

            if (usernameOrEmail != null)
            {
                bool result = _authService.SetNewPasswordAsync(newPassword, usernameOrEmail);

                if (result)
                {
                    _commonService.SetTempData("Password has been updated successfully.", "success");
                    return RedirectToAction("Login");
                }
            }

            _commonService.SetTempData("Failed to update password. Please try again.", "error");
            ModelState.AddModelError("", "Failed to update password. Please try again.");
            return View();
        }
    }
}
