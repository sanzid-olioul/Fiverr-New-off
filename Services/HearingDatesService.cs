using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles business logic and data access for conciliation hearing dates, including selecting, adding and deleting dates.
    /// </summary>
    public class HearingDatesService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;

        public HearingDatesService(PaLancCcdpDevDbContext context, CommonService commonService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
        }

        #region Case Comment Operations

        /// <summary>
        /// Retrieves a list of hearing dates.
        /// </summary>
        /// 
        public async Task<List<ConciliationHearingDateViewModel>> GetHearingDates()
        {
            var commentList = await _context.ConciliationHearingDates.ToListAsync();

            return commentList.Select(hd => new ConciliationHearingDateViewModel
            {
                HearingId = hd.HearingId,
                HearingDttm = hd.HearingDttm,
                CreatedDttm = hd.CreatedDttm,
                RecordStatus = hd.RecordStatus == "A" ? "Active" : "Deleted"
            }).OrderByDescending(hd => hd.HearingDttm.Date).ToList();
        }

        /// <summary>
        /// Adds dates to the Conciliation Hearing Dates Lookup Table.
        /// </summary>
        /// [HttpPost]
        public async Task<bool> SaveSelectedHearingDateTimes(List<DateTime>? hearingDatesTimes)
        {
            if (hearingDatesTimes != null && hearingDatesTimes.Count > 0)
            {
                foreach (var hearingDatetime in hearingDatesTimes)
                {
                    var newHearingDate = new ConciliationHearingDate
                    {
                        HearingDttm = hearingDatetime,  
                        CreatedUser = _sessionUser ?? "Unknown",
                        ModifiedUser = _sessionUser ?? "Unknown"
                    };
                    _context.ConciliationHearingDates.Add(newHearingDate);
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<DateTime>> GetExistingHearingDatesAsync(List<DateTime> hearingDatesTimes)
        {
            var existingDates = await _context.ConciliationHearingDates.Where(h => hearingDatesTimes.Contains(h.HearingDttm)).Select(h => h.HearingDttm).ToListAsync();

            return existingDates;
        }


        public async Task<bool> UpdateHearingDateTimeAsync(ConciliationHearingDateViewModel updatedDateTime)
        {
            var existingRecord = await _context.ConciliationHearingDates.Where(hd => hd.HearingId == updatedDateTime.HearingId).FirstOrDefaultAsync();
            if (existingRecord != null)
            {
                existingRecord.HearingDttm = updatedDateTime.HearingDttm;
                existingRecord.ModifiedUser = _sessionUser ?? "Unknown";
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Marks a conciliation hearing date as deleted by setting its RecordStatus.
        /// </summary>
        public async Task<bool> DeleteHearingDateAsync(int hearingId)
        {
            var existingCase = await _context.ConciliationHearingDates.Where(hd => hd.HearingId == hearingId).FirstOrDefaultAsync();
            if (existingCase == null) return false;

            existingCase.RecordStatus = "D";
            existingCase.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

    }
}