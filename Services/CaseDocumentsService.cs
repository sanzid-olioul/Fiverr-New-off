using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LancasterCreditCardDiversion.ViewModels;
using LancasterCreditCardDiversion.Models;
using System.Linq;
using Syncfusion.EJ2.Linq;
using Path = System.IO.Path;

namespace LancasterCreditCardDiversion.Services
{
    public class CaseDocumentsService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;
        private readonly CaseStatusClass _caseStatusClass;

        public CaseDocumentsService(PaLancCcdpDevDbContext context, CommonService commonService, IHttpContextAccessor httpContextAccessor, CaseStatusClass caseStatusClass)
        {
            _context = context;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            _caseStatusClass = caseStatusClass;
        }

        #region GET: List of Case Documents
        public async Task<List<CaseDocumentViewModel>> ListCaseDocumentsByIdAsync(string caseId,int page = 1, int pageSize = 10)
        {
            var query =
                from d in _context.CaseDocuments

                    // Join DOC_TYPE descriptions
                join adv in _context.AppDomainValues
                    on new { Code = d.DocType, Domain = "DOC_TYPE" }
                    equals new { Code = adv.Code, Domain = adv.DomainName }
                    into advJoin
                from adv in advJoin.DefaultIfEmpty()

                    // LEFT JOIN completed eligibility docs
                join erd in
                    (
                        from r in _context.ResponsesApiRequests
                        where r.CaseId == Convert.ToInt32(caseId)
                              && r.EligibilityCheckStatus == "completed"
                              && r.RecordStatus == "A"
                        join rd in _context.EligibilityCheckRequestDocuments
                            on r.ReqId equals rd.ReqId
                        select rd.DocId
                    )
                    on d.DocId equals erd into completedJoin
                from completed in completedJoin.DefaultIfEmpty()

                where d.CaseId == Convert.ToInt32(caseId)
                orderby d.CreatedDttm descending

                select new CaseDocumentViewModel
                {
                    DocId = d.DocId,
                    Name = d.Name,
                    DocDate = d.DocDate,
                    DocType = adv != null ? adv.Description : d.DocType,
                    CaseId = d.CaseId,
                    RecordStatus = d.RecordStatus == "A" ? "Active" : "Deleted",
                    CreatedDttm = d.CreatedDttm,
                    IsChecked =
                    _context.ResponsesApiRequests.Any(r =>
                        r.CaseId == Convert.ToInt32(caseId) &&
                        r.EligibilityCheckStatus == "completed" &&
                        r.RecordStatus == "A" &&
                        _context.EligibilityCheckRequestDocuments.Any(rd =>
                            rd.ReqId == r.ReqId &&
                            rd.DocId == d.DocId
                        )
                    )
                };

            return await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        #endregion

        #region Get Document By DocId
        public async Task<CaseDocumentViewModel?> GetCaseDocumentByIdAsync(int documentId)
        {
            var document = await _context.CaseDocuments.FindAsync(documentId);
            if (document == null) return null;

            return new CaseDocumentViewModel
            {
                DocId = document.DocId,
                CaseId = document.CaseId,
                Name = document.Name,
                Content = document.Content,
                DocType = document.DocType,
                DocTypeDomainName = document.DocTypeDomainName,
                RecordStatus = document.RecordStatus
            };
        }
        #endregion

        #region Delete Case Document
        public async Task<bool> DeleteCaseDocumentAsync(CaseDocumentViewModel caseDocToDelete)
        {
            var existingCaseDoc = await _context.CaseDocuments.FindAsync(caseDocToDelete.DocId);
            if (existingCaseDoc == null) return false;

            existingCaseDoc.RecordStatus = "D";
            existingCaseDoc.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Upload New Document
        public async Task<bool> UploadDocumentAsync(CaseDocumentViewModel caseDocumentViewModel, string? caseId)
        {
            if (caseDocumentViewModel.DocumentUpload != null && caseDocumentViewModel.DocumentUpload.Length > 0)
            {
                var strippedName = Path.GetFileNameWithoutExtension(caseDocumentViewModel.Name);
                var fileExtension = Path.GetExtension(caseDocumentViewModel.DocumentUpload.FileName).ToLower();
                var validFileTypes = await _context.AppDomainValues.Where(domainName => domainName.DomainName == "VALID_FILETYPE").Select(code => code.Code).ToListAsync();
                var validExtensions = validFileTypes.Select(fileType => "." + fileType.ToLower()).ToArray();

                if (validExtensions.Contains(fileExtension))
                {
                    using var outputStream = new MemoryStream();
                    await caseDocumentViewModel.DocumentUpload.CopyToAsync(outputStream);

                    //var domainName = await _context.AppDomainValues
                    //    .Where(type => type.Code == caseDocumentViewModel.DocType)
                    //    .Select(d => d.DomainName)
                    //    .FirstOrDefaultAsync();

                    var caseDoc = new CaseDocument
                    {
                        CaseId = (int)Convert.ToDecimal(caseId),
                        DocDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                        Name = $"{strippedName}{fileExtension}",
                        DocType = caseDocumentViewModel.DocType,
                        Content = outputStream.ToArray(),
                        DocTypeDomainName = "DOC_TYPE",
                        CreatedUser = _sessionUser ?? "Unknown",
                        ModifiedUser = _sessionUser ?? "Unknown"
                    };

                    _context.CaseDocuments.Add(caseDoc);
                    await _context.SaveChangesAsync();


                    // Update CaseStatus based on DocType
                    var caseStatusUpdateOnMerge = _caseStatusClass.CaseStatusUpdateOnMerge();

                    if (caseStatusUpdateOnMerge.ContainsKey(caseDocumentViewModel.DocType))
                    {
                        var caseById = await _context.CcdpCases
                            .Where(c => c.CaseId == Convert.ToDecimal(caseId))
                            .FirstOrDefaultAsync();

                        if (caseById != null)
                        {
                            caseById.CaseStatus = caseStatusUpdateOnMerge[caseDocumentViewModel.DocType];
                            caseById.ModifiedUser = _sessionUser ?? "Unknown";

                            await _context.SaveChangesAsync();
                        }
                    }

                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Update Document
        public async Task<bool> EditDocumentAsync(CaseDocumentViewModel model, IFormFile documentUpload)
        {
            var document = await _context.CaseDocuments.FindAsync(model.DocId);
            if (document == null) return false;

            var existingExtension = Path.GetExtension(document.Name);
            var strippedName = Path.GetFileNameWithoutExtension(model.Name);

            document.Name = $"{strippedName}{existingExtension}";
            document.DocType = model.DocType;
            document.RecordStatus = model.RecordStatus;
            document.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region GET: Document Download
        public async Task<IActionResult> DownloadDocumentAsync(int documentId)
        {
            var document = await _context.CaseDocuments.FindAsync(documentId);
            if (document == null) return new NotFoundResult();

            var documentViewModel = new CaseDocumentViewModel
            {
                DocId = document.DocId,
                Name = document.Name,
                Content = document.Content,
                DocType = document.DocType,
                DocTypeDomainName = document.DocTypeDomainName,
                CaseId = document.CaseId
            };

            return await _commonService.DownloadDocument(documentViewModel);
        }
        #endregion

        #region Document helpers
        public async Task<bool> IsPdfAsync(int documentId)
        {
            var document = await _context.CaseDocuments.FindAsync(documentId);
            if (document == null) return false;
            var extension = Path.GetExtension(document.Name)?.ToLower();
            return extension == ".pdf";
        }

        public async Task<byte[]?> GetDocumentBytesAsync(int documentId)
        {
            var document = await _context.CaseDocuments.FindAsync(documentId);
            return document?.Content;
        }
        #endregion

        #region Get Documents in Queue

        public async Task<List<EligibilityCheckRequest>> GetRequestsInQueueAsync(string caseId)
        {
            var queuedRequests = await _context.EligibilityCheckRequests.Where(r => r.CaseId == Convert.ToInt32(caseId) && (r.EligibilityCheckStatus == "queued" || r.EligibilityCheckStatus == "in_progress")).ToListAsync();

            return queuedRequests;
        }
        #endregion

        #region Get Results from API Request Response Table

        public async Task<(string Response, string? DocumentName)> GetResultFromAIAsync(string docId)
        {   
            var responseResult = "";

            var checkIfDocIdExists = await _context.EligibilityCheckRequestDocuments.Where(erd => erd.DocId == Convert.ToInt32(docId)).Select(erd => erd.ReqId).ToListAsync();

            if (checkIfDocIdExists != null && checkIfDocIdExists.Count > 0)
            {
                //var latestRequest = await _context.EligibilityCheckRequests.Where(er => checkIfDocIdExists.Contains(er.ReqId)).OrderByDescending(er => er.CreatedDttm).FirstOrDefaultAsync();
                var latestRequest = await _context.ResponsesApiRequests.Where(r => checkIfDocIdExists.Contains(r.ReqId)).OrderByDescending(r => r.CreatedDttm).FirstOrDefaultAsync();
                responseResult = latestRequest?.Response ?? "";
            }

            var docName = await _context.CaseDocuments.Where(cd => cd.DocId == Convert.ToInt32(docId)).Select(cd => cd.Name).FirstOrDefaultAsync();
            return (Response: responseResult, DocumentName: docName);
        }
        #endregion

        #region

        //public async Task AddDocumentToQueue(decimal caseId, List<CaseDocumentViewModel> documents)
        //{
        //    decimal reqId = 0;

        //    var newRequest = new EligibilityCheckRequest
        //    {
        //        CaseId = (int)caseId,
        //        AssistantId = "temp",
        //        ThreadId = "temp",
        //        EligibilityCheckStatus = "queued",
        //        CreatedUser = _sessionUser ?? "Unknown",
        //        ModifiedUser = _sessionUser ?? "Unknown",
        //    };
        //    _context.EligibilityCheckRequests.Add(newRequest);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        reqId = newRequest.ReqId;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error saving changes: " + ex.Message, ex);
        //    }

        //    foreach (var document in documents)
        //    {
        //        var getReqId = _context.EligibilityCheckRequests.Where(r => r.ReqId == reqId).FirstOrDefault();
        //        if (getReqId != null)
        //        {
        //            var saveDocPerRequestId = new EligibilityCheckRequestDocument
        //            {
        //                ReqId = getReqId.ReqId,
        //                DocId = (int)document.DocId
        //            };

        //            _context.EligibilityCheckRequestDocuments.Add(saveDocPerRequestId);
        //            try
        //            {
        //                 await _context.SaveChangesAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception("Error saving changes: " + ex.Message, ex);

        //            }
        //        }
                
        //    }
        //}

        public async Task AddDocumentToQueue(int caseId, List<CaseDocumentViewModel> documents)
        {
            // Create a placeholder ResponsesApiRequest row
            var newResponse = new ResponsesApiRequest
            {
                CaseId = caseId,
                PromptId = null,
                PromptVersion = null,
                Response = null,  // no AI response yet
                EligibilityCheckStatus = "queued",
                CreatedUser = _sessionUser ?? "Unknown",
                CreatedDttm = DateTime.UtcNow,
                ModifiedUser = _sessionUser ?? "Unknown",
                ModifiedDttm = DateTime.UtcNow,
                IsChecked = false
            };

            _context.ResponsesApiRequests.Add(newResponse);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving ResponsesApiRequest: " + ex.Message, ex);
            }

            var reqId = newResponse.ReqId;

            // 2. Insert join records for each document
            foreach (var document in documents)
            {
                var saveDocPerRequestId = new EligibilityCheckRequestDocument
                {
                    ReqId = reqId,
                    DocId = document.DocId
                };

                _context.EligibilityCheckRequestDocuments.Add(saveDocPerRequestId);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving EligibilityCheckRequestDocuments: " + ex.Message, ex);
            }
        }
        #endregion

    }
}