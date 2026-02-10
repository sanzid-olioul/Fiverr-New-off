using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using static NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles business logic and data access for check requests, listing the queued, in_progress, completed documents
    /// </summary>
    public class EligibilityCheckRequestsService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;

        public EligibilityCheckRequestsService(PaLancCcdpDevDbContext context, CommonService commonService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
        }

        #region Check Operations

        /// <summary>
        /// Retrieves a list of requests for a given case ID ordered by created date (latest on top)
        /// </summary>
        public async Task<List<EligibilityCheckRequestViewModel>> GetRequestsByIdAsync(string caseId)
        {
            if (!decimal.TryParse(caseId, out var id)) return new List<EligibilityCheckRequestViewModel>();

            var requestsList = await _context.EligibilityCheckRequests
                .Where(er => er.CaseId == Convert.ToInt32(caseId))
                .Select(er => new
                {
                    er.ThreadId,
                    Status = _context.AppDomainValues.Where(adv => adv.Code == er.EligibilityCheckStatus && adv.DomainName == er.EligibilityCheckStatusDomainName).Select(adv => adv.Description).FirstOrDefault(),
                    er.EligibilityCheckStatus,
                    er.CreatedDttm,
                    er.CreatedUser,
                    er.Response,
                    DocumentNames = _context.EligibilityCheckRequestDocuments.Where(erd => erd.ReqId == er.ReqId)
                        .Join(_context.CaseDocuments,
                            erd => erd.DocId,
                            cd => cd.DocId,
                            (erd, cd) => cd.Name)
                        .ToList()
                }).OrderByDescending(er => er.CreatedDttm)
                .ToListAsync();

            /* 
            To get document names for each thread
            
            SELECT cd.Name
            FROM EligibilityCheckRequests er
            JOIN EligibilityCheckReque_context.EligibilityCheckRequestDocuments.Name
            JOIN CaseDocuments cd ON cd.DocId = erd.DocId WHERE er.CaseId = @caseId; 
            
            */

            return requestsList.Select(r => new EligibilityCheckRequestViewModel
            {
                ThreadId = r.ThreadId,
                EligibilityCheckStatus = r.Status ?? "queued",
                CreatedDttm = r.CreatedDttm,
                CreatedUser = r.CreatedUser,
                IsChecked = r.Response != null,
                DocumentNames =  string.Join(", ", r.DocumentNames)
            }).ToList();
        }

        #endregion

        #region View Results from API

        public async Task<(string Response, string? DocumentNames)> GetAIResponseByIdAsync(string threadId)
        {
            var responseResult = await _context.EligibilityCheckRequests
                    .Where(r => r.ThreadId.Contains(threadId)).Select(r => r.Response).FirstOrDefaultAsync();  //Filter with date to get the topmost

            var getReqId = await _context.EligibilityCheckRequests.Where(r => r.ThreadId == threadId).Select(r => r.ReqId).FirstOrDefaultAsync();
            var reqDocIds = await _context.EligibilityCheckRequestDocuments.Where(rd => rd.ReqId == getReqId).Select(rd => rd.DocId).ToListAsync();
            var documentNames = await _context.CaseDocuments.Where(cd => reqDocIds.Contains(cd.DocId)).Select(cd => cd.Name).ToListAsync();
        
            string documentNamesString = string.Join(", ", documentNames);

            return (responseResult ?? "", documentNamesString);
        }
        #endregion

        #region Get Documents in Queue
        public async Task<List<ResponsesApiRequest>> GetRequestsInQueueAsync(string caseId)
        {

            //var queuedRequests = await _context.EligibilityCheckRequests.Where(r => r.CaseId == Convert.ToInt32(caseId) && (r.EligibilityCheckStatus == "queued" || r.EligibilityCheckStatus == "in_progress")).ToListAsync();
            var queuedRequests = await _context.ResponsesApiRequests.Where(r => r.CaseId == Convert.ToInt32(caseId) && (r.EligibilityCheckStatus == "queued" || r.EligibilityCheckStatus == "in_progress")).ToListAsync();

            return queuedRequests;
        }
        #endregion

        public async Task<List<ResponsesApiRequestsViewModel>> GetResponsesAPIByCaseIdAsync(string caseId)
        {
            if (!decimal.TryParse(caseId, out var id))
                return new List<ResponsesApiRequestsViewModel>();

            var responses = await _context.ResponsesApiRequests
                .Where(r => r.CaseId == id)
                .OrderByDescending(r => r.CreatedDttm)
                .Select(r => new ResponsesApiRequestsViewModel
                {
                    CaseId = r.CaseId,
                    ReqId = r.ReqId,
                    Response = r.Response,
                    EligibilityCheckStatus = r.EligibilityCheckStatus,
                    CreatedDttm = r.CreatedDttm,
                    CreatedUser = r.CreatedUser,
                    IsChecked = r.IsChecked,
                    DocumentNames = string.Join(", ",
                        _context.EligibilityCheckRequestDocuments
                            .Where(rd => rd.ReqId == r.ReqId)
                            .Join(_context.CaseDocuments,
                                  rd => rd.DocId,
                                  cd => cd.DocId,
                                  (rd, cd) => cd.Name)
                    )
                })
                .ToListAsync();

            return responses;
        }


        public async Task<(ResponsesApiRequest Response, string DocumentNames)?> GetResponseAPIByIdAsync(decimal caseId, decimal reqId)
        {
            var response = await _context.ResponsesApiRequests
                .FirstOrDefaultAsync(r => r.CaseId == caseId && r.ReqId == reqId);

            if (response == null) return null;

            var docIds = await _context.EligibilityCheckRequestDocuments
                .Where(rd => rd.ReqId == reqId)
                .Select(rd => rd.DocId)
                .ToListAsync();

            var documentNames = await _context.CaseDocuments
                .Where(cd => docIds.Contains(cd.DocId))
                .Select(cd => cd.Name)
                .ToListAsync();

            return (response, string.Join(", ", documentNames));
        }

    }
}