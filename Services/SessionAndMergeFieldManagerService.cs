using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LancasterCreditCardDiversion.Services
{
    public class SessionAndMergeFieldManagerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PaLancCcdpDevDbContext _context;

        public SessionAndMergeFieldManagerService(IHttpContextAccessor httpContextAccessor, PaLancCcdpDevDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        #region Session Management Utilities

        /// <summary>
        /// Retrieves the current case ID from session.
        /// </summary>
        public string? GetCurrentCaseId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("CurrentCaseId");
        }

        /// <summary>
        /// Sets multiple session values based on the given case view model.
        /// </summary>
        public async Task SetCaseSessionData(CcdpCaseViewModel ccdpCase, CaseDocumentViewModel caseDocuments)
        {
            string? cmoDateDay = null;
            string? ncoDateDay = null;
            string? rtsccoDateDay = null;
            string? cdplfDateDay = null;
            string? cdncoDateDay = null;
            string? formattedHearingDate = null;
            string? formattedHearingDatePrevious = null;

            var session = _httpContextAccessor.HttpContext?.Session;

            DateTime? hearingDate =  await _context.ConciliationHearingDates.Where(h => h.HearingId == ccdpCase.HearingId).Select(h => h.HearingDttm).FirstOrDefaultAsync();
            var getCaseHearingDates = await _context.CaseHearingDates
                           .Where(h => (decimal)h.CaseId == ccdpCase.CaseId) // Cast 'h.CaseId' to 'decimal' to match 'ccdpCase.CaseId'
                           .OrderByDescending(h => h.CreatedDttm)
                           .Skip(1)
                           .FirstOrDefaultAsync();

            DateTime? prevHearingDate = null;

            if (getCaseHearingDates != null)
            {
                prevHearingDate =await _context.ConciliationHearingDates.Where(h => h.HearingId == getCaseHearingDates.CaseHearingDttmId).Select(h => h.HearingDttm).FirstOrDefaultAsync();
            }

            var cmoDate = caseDocuments.Documents.Where(d => d.DocType == "CMO" && d.RecordStatus == "A").OrderByDescending(d => d.CreatedDttm).FirstOrDefault();
            var ncoDate = caseDocuments.Documents.Where(d => d.DocType == "NCO" && d.RecordStatus == "A").OrderByDescending(d => d.CreatedDttm).FirstOrDefault();
            var rtsccoDate = caseDocuments.Documents.Where(d => d.DocType == "RTSCCO" && d.RecordStatus == "A").OrderByDescending(d => d.CreatedDttm).FirstOrDefault();
            var cdplfDate = caseDocuments.Documents.Where(d => d.DocType == "CDPLF" && d.RecordStatus == "A").OrderByDescending(d => d.CreatedDttm).FirstOrDefault(); //Need to update based on DocName format
            var cdncoDate = caseDocuments.Documents.Where(d => d.DocType == "CDNCO" && d.RecordStatus == "A").OrderByDescending(d => d.CreatedDttm).FirstOrDefault(); //Need to update based on DocName format
            //Add if any other date merge fields required

            if (hearingDate.HasValue && hearingDate.Value != DateTime.MinValue)
            {
                formattedHearingDate = hearingDate.HasValue ? hearingDate.Value.ToString("MMMM dd, yyyy, 'at' h:mm tt").Replace("AM", "a.m.").Replace("PM", "p.m.") : "";
                formattedHearingDatePrevious = prevHearingDate.HasValue ? prevHearingDate.Value.ToString("MMMM dd, yyyy, 'at' 1:30 tt").Replace("AM", "a.m.").Replace("PM", "p.m.") : "";
            }

            if (cmoDate != null)
            {
                cmoDateDay = cmoDate.CreatedDttm.ToString("MMMM dd, yyyy");
            }
            if (ncoDate != null)
            {
                ncoDateDay = ncoDate.CreatedDttm.ToString("MMMM dd, yyyy");
            }
            if (rtsccoDate != null)
            {
                rtsccoDateDay = rtsccoDate.CreatedDttm.ToString("MMMM dd, yyyy");
            }
            if (cdplfDate != null)
            {
                cdplfDateDay = cdplfDate.CreatedDttm.ToString("MMMM dd, yyyy");
            }
            if (cdncoDate != null)
            {
                cdncoDateDay = cdncoDate.CreatedDttm.ToString("MMMM dd, yyyy");
            }

            int filingDay = ccdpCase.FilingDate.Day;

            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Define the EST time zone (Eastern Standard Time, without Daylight Saving Time)
            TimeZoneInfo estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            // Convert the UTC time to EST
            DateTime estNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, estTimeZone);

            // Get the day of the month in EST
            int todayDay = estNow.Day;

            // Calculate the next date when hearing falls on Tuesday or Wednesday
            string nextTuesdayWednesdayDate = GetNextTuesdayOrWednesday(estNow);

            string formattedFilingDate = ccdpCase.FilingDate.ToString("MMMM dd, yyyy"); //$"this {filingDay}{GetOrdinalSuffix(filingDay)} day of {ccdpCase.FilingDate.ToString("MMMM")}, {ccdpCase.FilingDate.Year}";
            string formattedDateToday = $"this {todayDay}{GetOrdinalSuffix(todayDay)} day of {estNow.ToString("MMMM")}, {estNow.Year}";

            session?.SetString("CurrentCaseId", ccdpCase.CaseId.ToString() ?? "<<CurrentCaseId>>");
            session?.SetString("CourtCaseNumber", ccdpCase.CourtCaseNumber ?? "<<CourtCaseNumber>>");
            session?.SetString("PlaintiffName", ccdpCase.PlaintiffName ?? "<<PlaintiffName>>");
            session?.SetString("PlaintiffRepName", ccdpCase.PlaintiffRepName ?? "<<PlaintiffRepName>>");
            session?.SetString("PlaintiffRep2Name", ccdpCase.PlaintiffRep2Name ?? "<<PlaintiffRep2Name>>");
            session?.SetString("DefendantName", ccdpCase.DefendantName ?? "<<DefendantName>>");
             session?.SetString("DefendantTwoName", ccdpCase.DefendantTwoName ?? "<<DefendantTwoName>>");
            session?.SetString("DefendantRepName", ccdpCase.DefendantRepName ?? "<<DefendantRepName>>");
            session?.SetString("DefendantRep2Name", ccdpCase.DefendantRep2Name ?? "<<DefendantRep2Name>>");
            session?.SetString("DefendantCopiesTo", !string.IsNullOrWhiteSpace(ccdpCase.DefendantRepName)? ccdpCase.DefendantRepName: (!string.IsNullOrWhiteSpace(ccdpCase.DefendantName) ? ccdpCase.DefendantName : "<<DefendantName>>"));
            session?.SetString("HearingDate", formattedHearingDate ?? "<<HearingDate>>");
            session?.SetString("HearingDatePrevious", formattedHearingDatePrevious ?? "<<HearingDatePrevious>>");
            session?.SetString("NextHearingTuesdayWednesday", nextTuesdayWednesdayDate ?? "<<NextHearingTuesdayWednesday>>");
            session?.SetString("FilingDate", formattedFilingDate ?? "<<FilingDate>>");
            session?.SetString("DateToday", formattedDateToday ?? "<<DateToday>>");
            session?.SetString("CMODate", cmoDateDay ?? "<<CMODate>>");
            session?.SetString("NCODate", ncoDateDay ?? "<<NCODate>>");
            session?.SetString("RTSCCODate", rtsccoDateDay ?? "<<RTSCCODate>>");
            session?.SetString("CDPLFDate", cdplfDateDay ?? "<<CDPLFDate>>");
            session?.SetString("CDNCODate", cdncoDateDay ?? "<<CDNCODate>>");
        }

        // Helper method to get the ordinal suffix
        string GetOrdinalSuffix(int day)
        {
            if (day < 1 || day > 31) return ""; // Handle invalid days

            if (day % 100 >= 11 && day % 100 <= 13) return "th"; // Special cases for 11, 12, 13

            switch (day % 10)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }

        // Helper method to get the next date when hearing falls on Tuesday or Wednesday
        string GetNextTuesdayOrWednesday(DateTime startDate)
        {
            DateTime current = startDate;
            
            // Keep incrementing the date until we find a Tuesday or Wednesday
            while (current.DayOfWeek != DayOfWeek.Tuesday && current.DayOfWeek != DayOfWeek.Wednesday)
            {
                current = current.AddDays(1);
            }

            // Format the date as "MMMM dd, yyyy"
            return current.ToString("MMMM dd, yyyy");
        }


        /// <summary>
        /// Retrieves all case-related details from the session as a dictionary.
        /// </summary>
        /// <returns>A dictionary containing key-value pairs of case-related session data.</returns>
        public Dictionary<string, string> GetCaseDetailsFromSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return new Dictionary<string, string>();
            }

            var keys = new[]
            {
            "CourtCaseNumber", "PlaintiffName", "DefendantName",
            "DefendantRepName", "DefendantRep2Name", "DefendantCopiesTo", "PlaintiffRepName",
            "PlaintiffRep2Name", "FilingDate", "HearingDate","HearingDatePrevious",
            "DateToday", "CMODate", "NCODate", "RTSCCODate",
            "CDPLFDate", "CDNCODate", "NextHearingTuesdayWednesday"
        };

            return keys.ToDictionary(key => key, key => session.GetString(key)!);
        }


        /// <summary>
        /// Clears all session data except username
        /// </summary>
        public void ClearSessionDataExceptUsername()
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session != null)
            {
                var username = session.GetString("Username");
                var fullName = session.GetString("FullName");

                var keys = session.Keys.ToList();
                foreach (var key in keys)
                {
                    if (key != "Username" && key != "FullName")
                    {
                        session.Remove(key);
                    }
                }

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(fullName))
                {
                    session.SetString("Username", username);
                    session.SetString("FullName", fullName);
                }
            }
        }

        #endregion
    }
}
