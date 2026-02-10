using DocumentFormat.OpenXml.Packaging;
using LancasterCreditCardDiversion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.PdfToImageConverter;
using System.Globalization;
using System.Text;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Provides shared functionalities for document management, domain lookups, dropdown list generation, and session handling.
    /// </summary>
    public class CommonService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly CaseStatusClass _caseStatusClass;

        public CommonService(PaLancCcdpDevDbContext context, IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataFactory, CaseStatusClass caseStatusClass)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _tempDataFactory = tempDataFactory;
            _caseStatusClass = caseStatusClass;
        }

        #region Document Operations

        /// <summary>
        /// Downloads a PDF document as a file response.
        /// </summary>
        public async Task<IActionResult> DownloadPdfDocument(IDocument doc)
        {
            return await DownloadDocument(doc);
        }

        /// <summary>
        /// Downloads a Word document as a file response.
        /// </summary>
        public async Task<IActionResult> DownloadWordDocument(IDocument? doc)
        {
            return await DownloadDocument(doc);
        }


        /// <summary>
        /// Downloads a document as a file response with appropriate content type based on file extension.
        /// </summary>
        public async Task<IActionResult> DownloadDocument(IDocument? doc)
        {
            if (doc == null) return new NotFoundResult();

            string? extension = Path.GetExtension(doc.Name)?.ToLower();
            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream",
            };

            byte[] content;

            if (extension == ".pdf")
            {
                content = await Task.FromResult(FlattenPdf(doc.Content!));
            }
            else
            {
                content = await Task.FromResult(doc.Content!);
            }

            return new FileContentResult(content, contentType) { FileDownloadName = doc.Name };
        }


        /// <summary>
        /// Flattens the PDF document while downloading
        /// </summary>
        public byte[] FlattenPdf(byte[] docContent)
        {
            if (docContent == null || docContent.Length == 0)
                throw new ArgumentException("PDF content is empty.");

            using (MemoryStream inputStream = new MemoryStream(docContent))
            {
                PdfToImageConverter imageConverter = new PdfToImageConverter();
                imageConverter.Load(inputStream);
                Stream[] outputStream = imageConverter.Convert(0, imageConverter.PageCount - 1, false, false);

                PdfDocument document = new PdfDocument();

                for (int i = 0; i < outputStream.Length; i++)
                {
                    PdfBitmap image = new PdfBitmap(outputStream[i]);
                    PdfSection section = document.Sections.Add();

                    PdfUnitConverter converter = new PdfUnitConverter();
                    float width = converter.ConvertUnits(image.PhysicalDimension.Width, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);
                    float height = converter.ConvertUnits(image.PhysicalDimension.Height, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);

                    section.PageSettings.Size = new Syncfusion.Drawing.SizeF(width, height);
                    section.PageSettings.Margins.All = 0;
                    PdfPage page = section.Pages.Add();

                    PdfGraphics graphics = page.Graphics;
                    graphics.DrawImage(image, 0, 0, width, height);
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    document.Save(memoryStream);
                    document.Close(true);
                    return memoryStream.ToArray();
                }
            }
        }
       
        /// <summary>
        /// Removes the file extension from the given file name.
        /// </summary>
        /// <param name="fileName">The file name from which to remove the extension.</param>
        /// <returns>The file name without its extension.</returns>
        public static string FileNameWithoutExtension(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        /// <summary>
        /// Convert file to text and count words
        /// </summary>
        public (string cleanedText, int wordCount) ReadFileContentAsync(byte[]? filePath, string docName)
        {
            var extension = Path.GetExtension(docName)?.ToLowerInvariant();
            StringBuilder text = new StringBuilder();
            if (extension == ".pdf" && filePath != null)
            {
                using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(filePath))
                {
                    for (int i = 0; i < loadedDocument.Pages.Count; i++)
                    {
                        PdfLoadedPage loadedPage = (PdfLoadedPage)loadedDocument.Pages[i];
                        text.AppendLine(loadedPage.ExtractText());
                    }
                }
            }
            else if (extension == ".docx" && filePath != null)
            {
                using (MemoryStream stream = new MemoryStream(filePath))
                {
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
                    {
                        var body = wordDoc?.MainDocumentPart?.Document.Body;
                        text.Append(body?.InnerText);
                    }
                }    
            }

            string cleanedText = text.ToString()
                   .Replace("\r\n\r\n", " ") // Replace double line breaks with a single space
                   .Replace("\r\n", " ")     // Replace single line breaks with a space
                   .Replace("\n", " ")       // Replace any new line characters
                   .Replace("\t", " ")       // Replace tabs with a space
                   .Trim();                  // Trim leading and trailing whitespace

            int wordCount = string.IsNullOrWhiteSpace(cleanedText) ? 0 : cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            return (cleanedText, wordCount);

        }

        #endregion

        #region Domain and Dropdown List Utilities

        /// <summary>
        /// Retrieves a list of values from the database based on a specific domain name, formatted as a dropdown select list.
        /// </summary>
        public async Task<List<SelectListItem>> GetDomainSelectListAsync(string domainName, string? selectedValue = null)
        {
            var values = await _context.AppDomainValues.Where(d => d.DomainName == domainName).OrderBy(v => v.Description).ToListAsync();

            return values.Select(v => new SelectListItem
            {
                Value = v.Code,
                Text = v.Description,
                Selected = v.Code == selectedValue
            }).ToList();
        }


        /// <summary>
        /// Retrieves hearing dates after a specified filing date, formatted as a dropdown select list.
        /// </summary>
        public async Task<List<SelectListItem>> GetHearingDatesAfterSetDaysAsync(DateTime filingDate)
        {
            var hearingDaysParameter = await _context.AppParameters.Where(p => p.Name == "SET_HEARING_AFTER_DAYS").Select(p => p.Value).FirstOrDefaultAsync();

            var cutoffDate = filingDate;
            if (hearingDaysParameter != null && int.TryParse(hearingDaysParameter, out var hearingDays))
            {
                cutoffDate = filingDate.AddDays(hearingDays);
            }

            var getActiveHearingDatesAfterCutoffDate = await _context.ConciliationHearingDates
                .Where(h => h.RecordStatus == "A" && h.HearingDttm > cutoffDate).OrderBy(h => h.HearingDttm).ToListAsync();

            var getCountOfCasesOnHearingDates = await _context.CcdpCases.Where(c => c.RecordStatus == "A").GroupBy(c => c.HearingId)
                        .Select(g => new
                        {
                            TotalCount = g.Count(),
                            HearingId = g.Key
                        }).ToListAsync();

            var caseCountDictionary = getCountOfCasesOnHearingDates.Where(c => c.HearingId != null).ToDictionary(c => Convert.ToInt32(c.HearingId), c => c.TotalCount);

            var hearingDates = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select a date" }
            };

            hearingDates.AddRange(getActiveHearingDatesAfterCutoffDate.Select(h => new SelectListItem
            {
                Value = h.HearingId.ToString(),
                Text = h.HearingDttm.ToString("MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture) + " (Cases: " + (caseCountDictionary.TryGetValue(Convert.ToInt32(h.HearingId), out int count) ? count.ToString() + ")" : "0 )")
            }));

            return hearingDates;
        }

        public async Task<List<SelectListItem>> GetAllHearingDatesAfterFilingDateListAsync(DateTime filingDate)
        {
            var getActiveHearingDatesAfterCutoffDate = await _context.ConciliationHearingDates.Where(h => h.RecordStatus == "A" && h.HearingDttm > filingDate).OrderBy(h => h.HearingDttm).ToListAsync();

            var getCountOfCasesOnHearingDates = await _context.CcdpCases.Where(c => c.RecordStatus == "A").GroupBy(c => c.HearingId)
                        .Select(g => new
                        {
                            TotalCount = g.Count(),
                            HearingId = g.Key
                        }).ToListAsync();

            var caseCountDictionary = getCountOfCasesOnHearingDates.Where(c => c.HearingId != null)
                     .ToDictionary(c => Convert.ToInt32(c.HearingId), c => c.TotalCount);

            var hearingDates = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select a date" }
            };

            hearingDates.AddRange(getActiveHearingDatesAfterCutoffDate.Select(h => new SelectListItem
            {
                Value = h.HearingId.ToString(),
                Text = h.HearingDttm.ToString("MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture) + " (Cases: " + (caseCountDictionary.TryGetValue(Convert.ToInt32(h.HearingId), out int count) ? count.ToString() + ")" : "0 )")
            }));

            return hearingDates;
        }

        /// <summary>
        /// Retrieves all hearing dates, formatted as a dropdown select list.
        /// </summary>
        //public async Task<List<SelectListItem>> GetAllHearingDatesSelectListAsync()
        //{
        //    var getAllHearingDates = await _context.ConciliationHearingDates.Where(h => h.RecordStatus == "A").ToListAsync();

        //    var getCountOfCasesOnHearingDates = await _context.CcdpCases.Where(c => c.RecordStatus == "A").GroupBy(c => c.HearingId)
        //      .Select(g => new
        //      {
        //          TotalCount = g.Count(),
        //          HearingId = g.Key
        //      }).ToListAsync();

        //    var caseCountDictionary = getCountOfCasesOnHearingDates.Where(c => c.HearingId != null)
        //             .ToDictionary(c => Convert.ToInt32(c.HearingId), c => c.TotalCount);

        //    var hearingDatesSelectList = getAllHearingDates.OrderByDescending(h => h.HearingDttm.Date).Select(h => new SelectListItem
        //    {
        //        Value = h.HearingId.ToString(),
        //        Text = h.HearingDttm.ToString("MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture) + " (Cases: " + (caseCountDictionary.TryGetValue(Convert.ToInt32(h.HearingId), out int count) ? count.ToString() + ")" : "0 )")
        //    }).ToList();

        //    hearingDatesSelectList.Insert(0, new SelectListItem
        //    {
        //        Value = "",
        //        Text = "All Hearings"  // Text indicating the "All" option
        //    });

        //    return hearingDatesSelectList;

        //}

        public async Task<List<SelectListItem>> GetAllHearingDatesSelectListAsync()
        {
            var list = await _context.ConciliationHearingDates
                .AsNoTracking()
                .Where(h => h.RecordStatus == "A")
                .OrderByDescending(h => h.HearingDttm)

                .Select(h => new
                {
                    h.HearingId,
                    h.HearingDttm,

                    CaseCount = _context.CcdpCases
                        .Where(c =>
                            c.RecordStatus == "A" &&
                            c.HearingId == h.HearingId
                        )
                        .Count()
                })

                .Select(x => new SelectListItem
                {
                    Value = x.HearingId.ToString(),
                    Text =
                        x.HearingDttm.ToString(
                            "MMM dd, yyyy, h:mm tt",
                            CultureInfo.InvariantCulture
                        ) +
                        $" (Cases: {x.CaseCount})"
                })
                .ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "All Hearings"
            });

            return list;
        }

        /// <summary>
        /// Retrieves the Hearing Dates Range for Search Page
        /// </summary>
        public async Task<List<SelectListItem>> GetHearingDateRangesAsync()
        {
            var hearingDateRanges = new List<SelectListItem>
            {
                new SelectListItem { Value = "LastMonth", Text = "Last Month" },
                new SelectListItem { Value = "Last3Months", Text = "Last 3 Months" },
                new SelectListItem { Value = "Last6Months", Text = "Last 6 Months" },
                new SelectListItem { Value = "Last12Months", Text = "Last 12 Months" },
                new SelectListItem { Value = "TodayFuture1Month", Text = "Today - Future 1 Month" },
                new SelectListItem { Value = "Future3Months", Text = "Future 3 Months" },
                new SelectListItem { Value = "Future6Months", Text = "Future 6 Months" },
                new SelectListItem { Value = "AllPast", Text = "All Past" },
                new SelectListItem { Value = "AllFuture", Text = "All Future" },
                new SelectListItem { Value = "NoDateSet" , Text = "No Date Set"}
            };

            return await Task.FromResult(hearingDateRanges);
        }


        /// <summary>
        /// Retrieves the domain name based on a provided status code.
        /// </summary>
        public async Task<string?> GetDomainNameAsync(string statusCodeOrDescription)
        {
            return await _context.AppDomainValues.Where(d => d.Code == statusCodeOrDescription || d.Description == statusCodeOrDescription).Select(d => d.DomainName).FirstOrDefaultAsync();
        }


        /// <summary>
        /// Retrieves the code of the Case Status based on a provided status description.
        /// </summary>
        public string? GetStatusCodeAsync(string statusInput)
        {
            statusInput = statusInput.Trim();
            var status =  _context.AppDomainValues.Where(d => d.Code == statusInput || d.Description == statusInput).Select(d => new { d.Code, d.Description }).FirstOrDefault();

            return status?.Code;
        }


        /// <summary>
        /// Retrieves the domain description based on a provided status code.
        /// </summary>
        public async Task<string?> GetDomainDescriptionAsync(string statusCode)
        {
            return await _context.AppDomainValues.Where(d => d.Code == statusCode).Select(d => d.Description).FirstOrDefaultAsync();
        }

        #endregion

        #region Data Retrieval Operations

        /// <summary>
        /// Retrieves records from the database based on the domain name parameter.
        /// </summary>
        public async Task<List<AppDomainValue>> GetAppDomainValuesAsync(string domainNameParameter)
        {
            return await _context.AppDomainValues.Where(domain => domain.DomainName == domainNameParameter).ToListAsync();
        }
        #endregion

        #region Set Temp Data Message
        /// <summary>
        /// Sets a temporary message to be displayed on the UI.
        /// </summary>
        public void SetTempData(string message, string messageType)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            var tempData = _tempDataFactory.GetTempData(httpContext);
            tempData["Message"] = message;
            tempData["MessageType"] = messageType;
        }
        #endregion

    }
}