using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles business logic and data access for cases, including creating, updating, deleting, and retrieving case information.
    /// </summary>
    public class CaseService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;
        private readonly CaseDocumentsService _caseDocumentsDataAccess;
        private readonly ILogger<CaseService> _logger;

        public CaseService(PaLancCcdpDevDbContext context, CommonService commonService, IHttpContextAccessor httpContextAccessor, CaseDocumentsService caseDocumentsDataAccess, ILogger<CaseService> logger)
        {
            _context = context;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            _caseDocumentsDataAccess = caseDocumentsDataAccess;
            _logger = logger;
        }

        #region Case Operations

        /// <summary>
        /// Retrieves a case by its ID.
        /// </summary>
        public async Task<CcdpCaseViewModel?> GetCaseByIdAsync(string caseId)
        {
            if (!decimal.TryParse(caseId, out var id)) return null;
            var caseEntity = await _context.CcdpCases.FirstOrDefaultAsync(c => c.CaseId == id);
            if (caseEntity == null) return null;
            var hearingDate = await _context.ConciliationHearingDates.Where(h => h.HearingId == caseEntity.HearingId).Select(h => h.HearingDttm).FirstOrDefaultAsync();
            var caseComment = await _context.CaseComments.Where(cc => cc.CaseId == caseEntity.CaseId && cc.RecordStatus == "A").OrderByDescending(cc => cc.CreatedDttm).Select(cc => cc.CommentText).FirstOrDefaultAsync();
            return new CcdpCaseViewModel
            {
                CaseId = caseEntity.CaseId,
                CourtCaseNumber = caseEntity.CourtCaseNumber,
                FilingDate = caseEntity.FilingDate,
                PlaintiffName = caseEntity.PlaintiffName,
                PlaintiffRepName = caseEntity.PlaintiffRepName,
                PlaintiffRep2Name = caseEntity.PlaintiffRep2Name,
                DefendantName = caseEntity.DefendantName,
                DefendantTwoName = caseEntity.DefendantTwoName,
                DefendantRepName = caseEntity.DefendantRepName,
                DefendantRep2Name = caseEntity.DefendantRep2Name,
                CaseStatus = caseEntity.CaseStatus,
                HearingId = caseEntity.HearingId,
                HearingDttm = hearingDate,
                CaseComment = caseComment,
                PlaintiffRepLawfirmName = caseEntity.PlaintiffRepLawfirmName,
                DefendantRepLawfirmName = caseEntity.DefendantRepLawfirmName
            };
        }

        /// <summary>
        /// Retrieves a list of cases based on a condition.
        /// </summary>
     
        public async Task<List<CcdpCaseViewModel>> ListConditionalCasesAsync(bool isActiveCase,int page = 1,int pageSize = 10)
        {
            var query =
                from c in _context.CcdpCases.AsNoTracking()

                    // Latest comment (LEFT JOIN via subquery)
                let latestComment =
                    (from cc in _context.CaseComments
                     where cc.CaseId == c.CaseId && cc.RecordStatus == "A"
                     orderby cc.CreatedDttm descending
                     select cc.CommentText).FirstOrDefault()

                // Hearing date
                join hd in _context.ConciliationHearingDates.AsNoTracking()
                    on c.HearingId equals hd.HearingId into hdJoin
                from hd in hdJoin.DefaultIfEmpty()

                    // Case status description
                join adv in _context.AppDomainValues.AsNoTracking()
                    on new { Code = c.CaseStatus, Domain = "CASE_STATUS" }
                    equals new { Code = adv.Code, Domain = adv.DomainName }
                    into advJoin
                from adv in advJoin.DefaultIfEmpty()

                let previousHearings =
                   (from ch in _context.CaseHearingDates.AsNoTracking()
                    join phd in _context.ConciliationHearingDates.AsNoTracking()
                       on ch.CaseHearingDttmId equals phd.HearingId
                    where ch.CaseId == c.CaseId
                          && ch.RecordStatus == "A"
                          && ch.CaseHearingDttmId != c.HearingId
                    orderby phd.HearingDttm descending
                    select phd.HearingDttm)
                   .ToList()

                where !isActiveCase || c.RecordStatus == "A"

                orderby c.CreatedDttm descending

                select new CcdpCaseViewModel
                {
                    CaseId = c.CaseId,
                    CourtCaseNumber = c.CourtCaseNumber,
                    FilingDate = c.FilingDate,

                    PlaintiffName = c.PlaintiffName,
                    PlaintiffRepName = c.PlaintiffRepName ?? "No Rep",
                    PlaintiffRep2Name = c.PlaintiffRep2Name ?? "No Rep",

                    DefendantName = c.DefendantName,
                    DefendantTwoName = c.DefendantTwoName ?? "No Second Defendant",
                    DefendantRepName = c.DefendantRepName ?? "No Rep",
                    DefendantRep2Name = c.DefendantRep2Name ?? "No Rep",

                    PlaintiffRepLawfirmName = c.PlaintiffRepLawfirmName ?? "None",
                    DefendantRepLawfirmName = c.DefendantRepLawfirmName ?? "None",

                    HearingId = c.HearingId,
                    HearingDttm = hd != null ? hd.HearingDttm: (DateTime?)null,

                    PreviousHearingDates = previousHearings.Any()
                        ? string.Join(", ",
                            previousHearings.Select(d =>
                                d.ToString("MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture)))
                        : "-",

                    CaseStatus = adv != null ? adv.Description.ToUpper() : "NEW",
                    CaseStatusCode = c.CaseStatus ?? "N",

                    CaseComment = latestComment ?? "No comment yet",

                    RecordStatus = c.RecordStatus == "A" ? "Active" : "Deleted",
                    RecordStatusCode = c.RecordStatus ?? "A",

                    CreatedDttm = c.CreatedDttm
                };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves case documents by ID.
        /// </summary>
        public async Task<CaseDocumentViewModel> GetCaseDocsByIdAsync(string caseId)
        {
            var caseDocs = await _context.CaseDocuments.Where(cd => cd.CaseId == Convert.ToDecimal(caseId)).OrderBy(cd => cd.CreatedDttm).Select(cd => new DocumentDto
            {
                DocName = cd.Name,
                CreatedDttm = cd.CreatedDttm,
                DocType = cd.DocType,
                RecordStatus = cd.RecordStatus
            }).ToListAsync();

            return new CaseDocumentViewModel
            {
                CaseId = (int)Convert.ToDecimal(caseId),
                Documents = caseDocs
            };
        }

        /// <summary>
        /// Saves a new case to the database.
        /// </summary>
        public async Task<(decimal? CaseId, bool IsCreated)> CreateCaseAsync(CcdpCaseViewModel caseToCreate)
        {
            var appDomainValue = await _commonService.GetDomainNameAsync(caseToCreate.CaseStatus);
            if (string.IsNullOrEmpty(appDomainValue)) return (null, false);

            var courtCaseExist = await _context.CcdpCases
                .FirstOrDefaultAsync(cc => cc.CourtCaseNumber == caseToCreate.CourtCaseNumber);

            if (courtCaseExist != null) return (null, false);

            var newCase = new CcdpCase
            {
                CourtCaseNumber = caseToCreate.CourtCaseNumber,
                FilingDate = caseToCreate.FilingDate,
                PlaintiffName = caseToCreate.PlaintiffName,
                PlaintiffRepName = caseToCreate.PlaintiffRepName,
                PlaintiffRep2Name = caseToCreate.PlaintiffRep2Name,
                DefendantName = caseToCreate.DefendantName,
                DefendantTwoName = caseToCreate.DefendantTwoName,
                DefendantRepName = caseToCreate.DefendantRepName,
                DefendantRep2Name = caseToCreate.DefendantRep2Name,
                PlaintiffRepLawfirmName = caseToCreate.PlaintiffRepLawfirmName,
                DefendantRepLawfirmName = caseToCreate.DefendantRepLawfirmName,
                CaseStatus = caseToCreate.CaseStatus,
                CaseStatusDomainName = appDomainValue,
                HearingId = (int?)caseToCreate.HearingId,
                RecordStatus = "A",
                CreatedUser = _sessionUser ?? "Unknown",
                ModifiedUser = _sessionUser ?? "Unknown"
            };

            await _context.CcdpCases.AddAsync(newCase);
            var caseResult = await _context.SaveChangesAsync();

            if (caseResult <= 0)
                return (null, false);

            // One upload field that may contain 0, 1, or many files.
            if (caseToCreate.AdditionalDocuments != null && caseToCreate.AdditionalDocuments.Length > 0)
            {
                var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                var nowEastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eastern);

                foreach (var file in caseToCreate.AdditionalDocuments)
                {
                    if (file == null || file.Length == 0) continue;

                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);

                    var document = new CaseDocument
                    {
                        Name = file.FileName,
                        DocDate = nowEastern,
                        Content = memoryStream.ToArray(),
                        DocType = "OT",
                        DocTypeDomainName = "DOC_TYPE",
                        CaseId = newCase.CaseId
                    };

                    _logger.LogInformation(
                        "Creating CaseDocument -> Domain='{Domain}', Code='{Code}', CaseId={CaseId}, File='{File}'",
                        document.DocTypeDomainName,
                        document.DocType,
                        document.CaseId,
                        document.Name
                    );

                    await _context.CaseDocuments.AddAsync(document);
                }

                await _context.SaveChangesAsync();
            }

            return (newCase.CaseId, true);
        }



        /// <summary>
        /// Updates an existing case with the provided details.
        /// </summary>
        public async Task<bool> UpdateCaseAsync(CcdpCaseViewModel caseToUpdate)
        {
            var appDomainValue = await _commonService.GetDomainNameAsync(caseToUpdate.CaseStatus);
            if (string.IsNullOrEmpty(appDomainValue)) return false;
            var existingCase = await _context.CcdpCases.FindAsync(caseToUpdate.CaseId);
            if (existingCase == null) return false;
            if (caseToUpdate.CaseComment != null)
            {
                var newComment = new CaseComment
                {
                    CaseId = (int)caseToUpdate.CaseId,
                    CommentText = caseToUpdate.CaseComment,
                    CreatedUser = _sessionUser ?? "Unknown",
                    ModifiedUser = _sessionUser ?? "Unknown"
                };
                await _context.CaseComments.AddAsync(newComment);
                await _context.SaveChangesAsync();
            }
            existingCase.CourtCaseNumber = caseToUpdate.CourtCaseNumber;
            existingCase.FilingDate = caseToUpdate.FilingDate;
            existingCase.PlaintiffName = caseToUpdate.PlaintiffName;
            existingCase.PlaintiffRepName = caseToUpdate.PlaintiffRepName;
            existingCase.PlaintiffRep2Name = caseToUpdate.PlaintiffRep2Name;
            existingCase.DefendantName = caseToUpdate.DefendantName;
            existingCase.DefendantTwoName = caseToUpdate.DefendantTwoName;
            existingCase.DefendantRepName = caseToUpdate.DefendantRepName;
            existingCase.DefendantRep2Name = caseToUpdate.DefendantRep2Name;
            existingCase.PlaintiffRepLawfirmName = caseToUpdate.PlaintiffRepLawfirmName;
            existingCase.DefendantRepLawfirmName = caseToUpdate.DefendantRepLawfirmName;
            existingCase.CaseStatus = caseToUpdate.CaseStatus;
            existingCase.CaseStatusDomainName = appDomainValue;
            existingCase.HearingId = caseToUpdate.HearingId > 0 ? caseToUpdate.HearingId : null; ;
            existingCase.ModifiedUser = _sessionUser ?? "Unknown";
            existingCase.RecordStatus = caseToUpdate.RecordStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates case management details, including hearing dates and case status.
        /// </summary>
        public async Task<bool> UpdateConciliationManagementAsync(CcdpCaseViewModel caseToUpdate)
        {
            //decimal? hearingId = null;
            decimal? hearingId = caseToUpdate.HearingId;

            //if (DateTime.TryParseExact(caseToUpdate.HearingDttm?.Trim(), "MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hearingDttm))
            //{
            //    hearingId = await _context.ConciliationHearingDates.Where(hd => hd.HearingDttm == hearingDttm).Select(hd => hd.HearingId).FirstOrDefaultAsync();
            //}
            //else if (int.TryParse(caseToUpdate.HearingDttm, out int hearingIdNumber))
            //{
            //    hearingId = hearingIdNumber;
            //}

            //if (hearingId == 0) hearingId = null;

            var appDomainValue = await _commonService.GetDomainNameAsync(caseToUpdate.CaseStatus);
            if (string.IsNullOrEmpty(appDomainValue)) return false;

            var existingCase = await _context.CcdpCases.FindAsync(caseToUpdate.CaseId);
            if (existingCase == null) return false;

            var caseCode =  _commonService.GetStatusCodeAsync(caseToUpdate.CaseStatus);
            var existingComment = await _context.CaseComments.Where(cc => cc.CaseId == existingCase.CaseId && cc.RecordStatus == "A").OrderByDescending(cc => cc.CreatedDttm).Select(cc => cc.CommentText).FirstOrDefaultAsync();

            if (caseToUpdate.CaseComment != null && caseToUpdate.CaseComment != existingComment && caseToUpdate.CaseComment != "No comment yet")
            {
                var newComment = new CaseComment
                {
                    CaseId = (int)caseToUpdate.CaseId,
                    CommentText = caseToUpdate.CaseComment
                };
                await _context.CaseComments.AddAsync(newComment);
                await _context.SaveChangesAsync();
            }

            //existingCase.CourtCaseNumber = caseToUpdate.CourtCaseNumber;
            existingCase.FilingDate = caseToUpdate.FilingDate;
            existingCase.PlaintiffName = caseToUpdate.PlaintiffName;
            existingCase.PlaintiffRepName = caseToUpdate.PlaintiffRepName;
            existingCase.PlaintiffRep2Name = caseToUpdate.PlaintiffRep2Name;
            existingCase.DefendantName = caseToUpdate.DefendantName;
            existingCase.PlaintiffRepLawfirmName = caseToUpdate.PlaintiffRepLawfirmName;
            existingCase.DefendantRepLawfirmName = caseToUpdate.DefendantRepLawfirmName;
            existingCase.CaseStatus = caseCode ?? "N";
            existingCase.CaseStatusDomainName = appDomainValue;
            existingCase.HearingId = (int?)hearingId;
            existingCase.ModifiedUser = _sessionUser ?? "Unknown";
            existingCase.RecordStatus = (caseToUpdate.RecordStatus == "Active" || caseToUpdate.RecordStatus == "A") ? "A" : "D";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError("Error Saving Conciliation Data update: {ErrorMessage}", ex.Message);
                return false;
            }


            return true;
        }


        /// <summary>
        /// Retrieves all comments for a specific case ID.
        /// </summary>
        public async Task<List<string>> GetAllCommentsByCaseId(int caseId)
        {
            var allComments = await _context.CaseComments.Where(c => c.CaseId == caseId).OrderByDescending(c => c.CreatedDttm).Select(c => new { c.CreatedUser, c.CreatedDttm, c.CommentText }).ToListAsync();
            return allComments.Select(c => $"[{c.CreatedUser} {c.CreatedDttm:yyyy-MM-dd hh:mm tt}] {c.CommentText}").ToList();
        }

        /// <summary>
        /// Marks a case as deleted by setting its RecordStatus.
        /// </summary>
        public async Task<bool> DeleteCaseAsync(decimal caseId)
        {
            var existingCase = await _context.CcdpCases.FindAsync(Convert.ToInt32(caseId));
            if (existingCase == null) return false;
            existingCase.RecordStatus = "D";
            existingCase.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a list of activity logs for a given case ID.
        /// </summary>
        public async Task<List<CaseHistoryViewModel>> GetActivityLogsByCaseIdAsync(string caseId)
        {
            var caseHistory = await _context.CaseHistories.Where(log => log.CaseId == Convert.ToDecimal(caseId)).OrderByDescending(log => log.EventDate).ToListAsync();
            var eventTypeDescriptions = await _context.AppDomainValues.Where(adv => caseHistory.Select(c => c.EventType).Contains(adv.Code)).ToDictionaryAsync(adv => adv.Code, adv => adv.Description);
            return caseHistory.Select(c => new CaseHistoryViewModel
            {
                CaseHistoryId = c.CaseHistoryId,
                CaseId = c.CaseId,
                EventType = eventTypeDescriptions.TryGetValue(c.EventType, out var description) ? description : "",
                EventTypeDomainName = c.EventTypeDomainName,
                EventDate = c.EventDate,
                Description = c.Description,
                RecordStatus = c.RecordStatus == "A" ? "Active" : "Deleted"
            }).ToList();
        }

        /// <summary>
        /// Searches for cases based on specified criteria.
        /// </summary>
        public async Task<List<CcdpCaseViewModel>> SearchCasesAsync(CcdpCaseViewModel criteria)
        {
            var query = _context.CcdpCases.AsQueryable();
            if (!string.IsNullOrEmpty(criteria.CourtCaseNumber)) query = query.Where(c => c.CourtCaseNumber.Contains(criteria.CourtCaseNumber));
            if (!string.IsNullOrEmpty(criteria.FilingDateSearchValue) && DateTime.TryParse(criteria.FilingDateSearchValue, out DateTime filingDate)) query = query.Where(c => c.FilingDate.Date == filingDate.Date);
            if (!string.IsNullOrEmpty(criteria.CaseStatus)) query = query.Where(c => c.CaseStatus.Contains(criteria.CaseStatus));
            if (!string.IsNullOrEmpty(criteria.PlaintiffName)) query = query.Where(c => c.PlaintiffName.ToLower().Contains(criteria.PlaintiffName.ToLower()));
            if (!string.IsNullOrEmpty(criteria.PlaintiffRepName))
            {
                var searchTerm = criteria.PlaintiffRepName.ToLower();
                query = query.Where(c => (c.PlaintiffRepName != null && c.PlaintiffRepName.ToLower().Contains(searchTerm)) || (c.PlaintiffRep2Name != null && c.PlaintiffRep2Name.ToLower().Contains(searchTerm)));
            }
            if (!string.IsNullOrEmpty(criteria.DefendantName)) query = query.Where(c => c.DefendantName.ToLower().Contains(criteria.DefendantName.ToLower()));
            if (!string.IsNullOrEmpty(criteria.DefendantRepName))
            {
                var searchTerm = criteria.DefendantRepName.ToLower();
                query = query.Where(c => (c.DefendantRepName != null && c.DefendantRepName.ToLower().Contains(searchTerm)) || (c.DefendantRep2Name != null && c.DefendantRep2Name.ToLower().Contains(searchTerm)));
            }
            if (!string.IsNullOrEmpty(criteria.HearingDateRange))
            {
                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;
                switch (criteria.HearingDateRange)
                {
                    case "LastMonth": startDate = endDate.AddMonths(-1); break;
                    case "Last3Months": startDate = endDate.AddMonths(-3); break;
                    case "Last6Months": startDate = endDate.AddMonths(-6); break;
                    case "Last12Months": startDate = endDate.AddMonths(-12); break;
                    case "TodayFuture1Month": startDate = endDate; endDate = endDate.AddMonths(1); break;
                    case "Future3Months": startDate = endDate; endDate = endDate.AddMonths(3); break;
                    case "Future6Months": startDate = endDate; endDate = endDate.AddMonths(6); break;
                    case "AllPast": startDate = DateTime.MinValue; break;
                    case "AllFuture": startDate = endDate; endDate = DateTime.MaxValue; break;
                    case "NoDateSet": query = query.Where(c => c.HearingId == null); break;
                }
                var hearingIds = await _context.ConciliationHearingDates.Where(hd => hd.HearingDttm >= startDate && hd.HearingDttm <= endDate).Select(hd => (decimal?)hd.HearingId).ToListAsync();
                if (hearingIds != null) query = query.Where(c => hearingIds.Contains(c.HearingId));
            }
            var result = await query.ToListAsync();
            var hearingDates = await _context.ConciliationHearingDates.Where(hd => result.Select(c => c.HearingId).Contains(hd.HearingId)).ToListAsync();
            var caseComments = await _context.CaseComments.Where(cc => result.Select(c => c.CaseId).Contains(cc.CaseId)).ToListAsync();
            return result.Select(c => new CcdpCaseViewModel
            {
                CaseId = c.CaseId,
                CourtCaseNumber = c.CourtCaseNumber,
                HearingId = c.HearingId,
                HearingDttm = hearingDates.Find(hd => hd.HearingId == c.HearingId)?.HearingDttm,
                FilingDate = c.FilingDate,
                FilingDateSearchValue = c.FilingDate.ToString("MMM dd, yyyy"),
                PlaintiffName = c.PlaintiffName,
                PlaintiffRepName = c.PlaintiffRepName ?? "No Rep",
                PlaintiffRep2Name = c.PlaintiffRep2Name ?? "No Rep",
                DefendantName = c.DefendantName,
                DefendantRepName = c.DefendantRepName ?? "No Rep",
                DefendantRep2Name = c.DefendantRep2Name ?? "No Rep",
                CaseStatus = _commonService.GetDomainDescriptionAsync(c.CaseStatus).Result?.ToUpper() ?? "NEW",
                CaseComment = caseComments.OrderByDescending(cc => cc.CreatedDttm).FirstOrDefault(cc => cc.CaseId == c.CaseId)?.CommentText ?? "No comment yet",
                RecordStatus = c.RecordStatus == "A" ? "Active" : "Deleted",
                CreatedDttm = c.CreatedDttm,
            }).OrderByDescending(c => c.CreatedDttm).ToList();
        }

        #endregion
    }
}
