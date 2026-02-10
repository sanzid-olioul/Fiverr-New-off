using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Text;


namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles business logic and database operations for letter templates.
    /// </summary>
    public class LetterTemplatesService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;
        private readonly CaseStatusClass _caseStatusClass;

        public LetterTemplatesService(PaLancCcdpDevDbContext context, CommonService documentService, IHttpContextAccessor httpContextAccessor, CaseStatusClass caseStatusClass)
        {
            _context = context;
            _commonService = documentService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _caseStatusClass = caseStatusClass;
        }

        public async Task<List<LetterTemplateViewModel>> ListTemplatesAsync(bool? ifdropdown = null)
        {
            List<LetterTemplate> letterTemp;

            if (ifdropdown == true)
            {
                letterTemp = await _context.LetterTemplates.Where(t => t.RecordStatus == "A").ToListAsync();
            }
            else
            {
                letterTemp = await _context.LetterTemplates.ToListAsync();
            }
            
            var docTypeDescriptions = await _context.AppDomainValues
                .Where(adv => adv.DomainName == "DOC_TYPE" && letterTemp.Select(c => c.DocType).Contains(adv.Code))
                .ToDictionaryAsync(adv => adv.Code, adv => adv.Description);


            return letterTemp.Select(t => new LetterTemplateViewModel
            {
                LetterTemplateId = t.LetterTemplateId,
                Name = t.Name,
                PublishedDate = t.PublishedDate,
                Content = t.Content,
                DocType = docTypeDescriptions.TryGetValue(t.DocType, out var description) ? description : "",
                DocTypeDomainName = t.DocTypeDomainName,
                ConvertToPdf = t.ConvertToPdf,
                RecordStatus = t.RecordStatus == "A" ? "Active" : "Deleted"
            }).OrderBy(t => t.Name).ToList();
        }

        public async Task<byte[]> MergeTemplateAsync(LetterTemplateViewModel letterTemplateViewModel, string? caseId, Dictionary<string, string> caseDetails, bool convertPdf)
        {
            var fileExtension = convertPdf ? ".pdf" : ".docx";
            var namesList = await _context.CaseDocuments.Where(l => l.CaseId == Convert.ToInt32(caseId) && l.RecordStatus == "A")
                .Select(l => l.Name.Trim().ToUpper()).ToListAsync();
            string searchName = (letterTemplateViewModel.Name + fileExtension).Trim().ToUpper();
            bool nameExists = namesList.Contains(searchName);

            if (nameExists)
            {
                return new byte[0];
            }

            var template = await _context.LetterTemplates.FindAsync(letterTemplateViewModel.LetterTemplateId);
            if (template == null) throw new InvalidOperationException("Template not found");

            using var document = new WordDocument(new MemoryStream(template.Content), FormatType.Docx);
            var keys = caseDetails.Select(kvp => kvp.Key).ToArray();
            var values = caseDetails.Select(kvp => kvp.Value).ToArray();

            document.MailMerge.MergeField += new MergeFieldEventHandler(ApplyTextColor);

            document.MailMerge.Execute(keys, values);

            var outputStream = new MemoryStream();
            if (convertPdf)
            {
                using var renderer = new DocIORenderer();
                using var pdfDocument = renderer.ConvertToPDF(document);
                pdfDocument.Save(outputStream);
                pdfDocument.Close(true);
            }
            else
            {
                document.Save(outputStream, FormatType.Docx);
            }

            byte[] documentContent = outputStream.ToArray();

            var docTypeCode = await _context.AppDomainValues.Where(adv => adv.Description == letterTemplateViewModel.DocType).Select(adv => adv.Code).FirstOrDefaultAsync();

            if (docTypeCode == null)
            {
                throw new InvalidOperationException("Document type code not found.");
            }

            var caseDoc = new CaseDocument
            {
                CaseId = (int)Convert.ToDecimal(caseId),
                DocDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                Name = Path.GetFileNameWithoutExtension(letterTemplateViewModel.Name) + fileExtension,
                DocType = docTypeCode,
                Content = documentContent,
                DocTypeDomainName = letterTemplateViewModel.DocTypeDomainName,
                CreatedUser = _sessionUser ?? "Unknown",
                ModifiedUser = _sessionUser ?? "Unknown"
            };

            _context.CaseDocuments.Add(caseDoc);
            await _context.SaveChangesAsync();

            var caseStatusUpdateOnMerge = _caseStatusClass.CaseStatusUpdateOnMerge();

            if (caseStatusUpdateOnMerge.ContainsKey(docTypeCode))
            {
                var caseById = await _context.CcdpCases.Where(c => c.CaseId == Convert.ToDecimal(caseId)).FirstOrDefaultAsync();

                if (caseById != null)
                {
                    caseById.CaseStatus = caseStatusUpdateOnMerge[docTypeCode];
                    caseById.ModifiedUser = _sessionUser ?? "Unknown";

                    await _context.SaveChangesAsync();
                }
            }

            return documentContent;
        }

        private static void ApplyTextColor(object sender, MergeFieldEventArgs e)
        {
            if (e.TextRange.Text.Contains("<<") && e.TextRange.Text.Contains(">>"))
            {
                e.TextRange.CharacterFormat.TextColor = Syncfusion.Drawing.Color.Blue;
            }
        }

        public async Task<IActionResult> DownloadWordDocument(int id)
        {
            var letterTemplate = await _context.LetterTemplates.FindAsync(id);
            if (letterTemplate == null) return new NotFoundResult();

            var viewModel = new LetterTemplateViewModel
            {
                LetterTemplateId = letterTemplate.LetterTemplateId,
                Name = letterTemplate.Name,
                Content = letterTemplate.Content,
                DocType = letterTemplate.DocType
            };

            return await _commonService.DownloadWordDocument(viewModel);

        }

        public async Task<bool> CreateTemplateAsync(LetterTemplateViewModel letterTemplateViewModel, string plaintiffNameField, string caseDescriptionField)
        {
            var checkName = _context.LetterTemplates.Where(l => l.Name.ToUpper() == (letterTemplateViewModel.Name + ".docx").ToUpper()).FirstOrDefault();
            if (checkName != null)
            {
                return false;
            }

            //var domainName = await _context.AppDomainValues
            //    .Where(type => type.Code == letterTemplateViewModel.DocType)
            //    .Select(d => d.DomainName)
            //    .FirstOrDefaultAsync();

            var letterTemplate = new LetterTemplate
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(letterTemplateViewModel.Name) + ".docx",
                Content = await GetTemplateContentAsync(letterTemplateViewModel),
                ConvertToPdf = letterTemplateViewModel.isConvertToPdf == true ? "Y" : "N",
                DocType = letterTemplateViewModel.DocType,
                DocTypeDomainName = "DOC_TYPE",
                CreatedUser = _sessionUser ?? "Unknown",
                ModifiedUser= _sessionUser ?? "Unknown"
            };

            _context.LetterTemplates.Add(letterTemplate);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<byte[]> GetTemplateContentAsync(LetterTemplateViewModel letterTemplateViewModel)
        {
            if (letterTemplateViewModel.TemplateFile != null && letterTemplateViewModel.TemplateFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await letterTemplateViewModel.TemplateFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }

            using var stream = new MemoryStream();

            return stream.ToArray();
        }

        public async Task<LetterTemplateViewModel?> GetTemplateByIdAsync(decimal id)
        {
            return await _context.LetterTemplates
                .Where(template => template.LetterTemplateId == id)
                .Select(template => new LetterTemplateViewModel
                {
                    LetterTemplateId = template.LetterTemplateId,
                    Name = template.Name,
                    PublishedDate = template.PublishedDate,
                    Content = template.Content,
                    DocType = template.DocType,
                    DocTypeDomainName = template.DocTypeDomainName,
                    isConvertToPdf = template.ConvertToPdf == "Y" ? true : false,
                    RecordStatus = template.RecordStatus
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EditTemplateAsync(LetterTemplateViewModel model, IFormFile templateFile)
        {
            var template = await _context.LetterTemplates.FindAsync(model.LetterTemplateId);
            if (template == null) return false;

            if (templateFile != null && templateFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await templateFile.CopyToAsync(memoryStream);
                template.Content = memoryStream.ToArray();
            }

            template.Name = System.IO.Path.GetFileNameWithoutExtension(model.Name) + ".docx";
            template.DocType = model.DocType;
            template.ConvertToPdf = model.isConvertToPdf == true ? "Y" : "N";
            template.ModifiedUser = _sessionUser ?? "Unknown";
            template.RecordStatus = model.RecordStatus ?? "A";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(decimal templateId)
        {
            var template = await _context.LetterTemplates.FindAsync(templateId);
            if (template == null) return false;

            template.RecordStatus = "D";
            template.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SelectListItem>> GetDocTypeSelectListAsync(string? selectedDocType)
        {
            var docTypes = await _context.AppDomainValues.Where(d => d.DomainName == "DOC_TYPE").ToListAsync();

            return docTypes.Select(d => new SelectListItem
            {
                Value = d.Code,
                Text = d.Description,
                Selected = d.Code == selectedDocType
            }).OrderBy(d => d.Text).ToList();
        }
    }
}