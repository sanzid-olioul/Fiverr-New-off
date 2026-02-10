using System.Threading.Tasks;
using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to manage user-related operations, including listing, creating, editing, and deleting users.
    /// </summary>
    public class UsersController : BaseController
    {
        private readonly UserService _userService;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public UsersController(UserService userService, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _userService = userService;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
        }

        /// <summary>
        /// Displays a list of all users.
        /// </summary>
        public async Task<IActionResult> ListUsers()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            var users = await _userService.ListUsersAsync();
            return View(users);
        }

        /// <summary>
        /// Returns the view to create a new user.
        /// </summary>
        public IActionResult CreateUser()
        {
            var model = new UserViewModel();
            return View(model);
        }

        /// <summary>
        /// Creates a new user based on the provided details.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            var success = await _userService.CreateUser(model);

            if (success)
            {
                _commonService.SetTempData("Successfully created the user", "success");
                return RedirectToAction("ListUsers");
            }

            _commonService.SetTempData("Failed to create the user, it may already exist", "error");
            return View(model);
        }

        /// <summary>
        /// Returns the view to edit an existing user by username.
        /// </summary>
        [HttpGet("Users/EditUser/{username}")]
        public async Task<IActionResult> EditUser(string username)
        {
            var userDetails = await _userService.GetUserDetails(username);
            if (userDetails == null)
            {
                return NotFound();
            }

            return View(userDetails);
        }

        /// <summary>
        /// Deletes a user based on the provided username.
        /// </summary>
        [HttpPost("Users/DeleteConfirmed/{username}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string username)
        {
            var isDeleted = await _userService.DeleteUserAsync(username);

            if (isDeleted)
            {
                _commonService.SetTempData("User deleted successfully", "success");
            }
            else
            {
                _commonService.SetTempData("Failed to delete the user.", "error");
                return NotFound();
            }

            return RedirectToAction("ListUsers");
        }
    }
}
