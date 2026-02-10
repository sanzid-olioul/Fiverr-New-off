using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;

namespace LancasterCreditCardDiversion.Controllers
{
    /// <summary>
    /// Controller to manage hearing dates, including listing, adding, updating, and deleting dates.
    /// </summary>
    public class HearingDatesController : BaseController
    {
        private readonly HearingDatesService _hearingDatesService;
        private readonly CommonService _commonService;
        private readonly SessionAndMergeFieldManagerService _sessionMergeService;

        public HearingDatesController(HearingDatesService hearingDatesService, CommonService commonService, SessionAndMergeFieldManagerService sessionMergeService)
        {
            _hearingDatesService = hearingDatesService;
            _commonService = commonService;
            _sessionMergeService = sessionMergeService;
        }

        /// <summary>
        /// Displays a list of hearing dates in a grid view.
        /// </summary>
        public async Task<IActionResult> ListHearingDates()
        {
            _sessionMergeService.ClearSessionDataExceptUsername();
            var hearingDates = await _hearingDatesService.GetHearingDates();
            return View(hearingDates);
        }

        /// <summary>
        /// Displays the view to select hearing dates.
        /// </summary>
        public IActionResult SelectHearingDates()
        {
            return View();
        }

        /// <summary>
        /// Saves the selected hearing dates and times.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SelectHearingDates(string selectedDatesAndTimes)
        {
            if (!string.IsNullOrEmpty(selectedDatesAndTimes))
            {
                var dateTimeStrings = JsonConvert.DeserializeObject<List<string>>(selectedDatesAndTimes);
                var hearingDatesTimes = dateTimeStrings?.Select(dt =>
                {
                    return DateTime.Parse(dt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }).ToList();

                var existingDates = await _hearingDatesService.GetExistingHearingDatesAsync(hearingDatesTimes!);

                if (existingDates != null && existingDates.Count > 0)
                {
                    _commonService.SetTempData(
                        $"Duplicate record(s) found. The following dates already exist: {string.Join(", ", existingDates)}",
                        "error"
                    );
                    return View();
                }

                var success = await _hearingDatesService.SaveSelectedHearingDateTimes(hearingDatesTimes);
                if (success)
                {
                    _commonService.SetTempData("Successfully added date(s)", "success");
                    return RedirectToAction("ListHearingDates");
                }
            }

            _commonService.SetTempData("Failed to save hearing dates and times.", "error");
            return View();
        }

        /// <summary>
        /// Updates an existing hearing date and time.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateHearingDateTime([FromBody] ConciliationHearingDateViewModel hearingDateTime)
        {
            if (hearingDateTime == null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            hearingDateTime.HearingDttm = hearingDateTime.HearingDttm.ToLocalTime();
            var isUpdated = await _hearingDatesService.UpdateHearingDateTimeAsync(hearingDateTime);

            return Json(new
            {
                success = isUpdated,
                message = isUpdated ? "Updated successfully" : "Failed to update"
            });
        }

        /// <summary>
        /// Deletes a hearing date based on the provided ID.
        /// </summary>
        [HttpPost("HearingDates/DeleteConfirmed/{hearingId}")]
        public async Task<IActionResult> DeleteConfirmed(int hearingId)
        {
            var isDeleted = await _hearingDatesService.DeleteHearingDateAsync(hearingId);

            if (isDeleted)
            {
                _commonService.SetTempData("Date deleted successfully", "success");
            }
            else
            {
                _commonService.SetTempData("Failed to delete the date.", "error");
                return NotFound();
            }

            return RedirectToAction("ListHearingDates");
        }
    }
}
