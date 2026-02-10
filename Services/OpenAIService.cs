//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Syncfusion.EJ2.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.ClientModel;
//using LancasterCreditCardDiversion.ViewModels;
//using LancasterCreditCardDiversion.Models;

//namespace LancasterCreditCardDiversion.Services
//{
//    /// <summary>
//    /// Handles Open AI Services
//    /// </summary>
//    public class OpenAIService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string? _apiKey;
//        private readonly string _apiBaseUrl;
//        private readonly string _modelName;
//        private readonly CommonService _commonService;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly string? _sessionUser;
//        private readonly PaLancCcdpDevDbContext? _context;
//        private readonly ILogger<TimeHostedCheckEligibilityService> _logger;
//        private readonly string _responseAPIPromptId;
//        private readonly string _promptText;

//        public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<OpenAIConfigViewModel> options, CommonService commonService, IHttpContextAccessor httpContextAccessor, PaLancCcdpDevDbContext context, ILogger<TimeHostedCheckEligibilityService> logger)
//        {
//            _httpClient = new HttpClient();
//            _apiKey = options.Value.ApiKey;
//            _apiBaseUrl = options.Value.ApiBaseUrl;
//            _modelName = options.Value.ModelName;
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            _httpClient.DefaultRequestHeaders.Add("x-ms-client-timeout", "600");
//            _commonService = commonService;
//            _httpContextAccessor = httpContextAccessor;
//            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
//            _context = context;
//            _logger = logger;
//            _responseAPIPromptId = configuration["ResponseAPIPromptId"] ?? throw new ArgumentNullException("ResponseAPIPromptId configuration is missing");
//            _promptText = Prompt.EligibilityPrompt;
//        }

//        public async Task CheckDocumentEligibility(List<CaseDocumentViewModel> documents, decimal? reqId)
//        {
//            var endpoint = new Uri(_apiBaseUrl);
//            //var apiKey = new AzureKeyCredential(_apiKey!);
//            var apiKey = new ApiKeyCredential(_apiKey!);

//            var requestBody = new { };

//            var json = JsonSerializer.Serialize(requestBody);
//            var encodedJson = new StringContent(json, Encoding.UTF8, "application/json");

//            var createdVectorStore = await _httpClient.PostAsync($"{_apiBaseUrl}/vector_stores", encodedJson);
//            createdVectorStore.EnsureSuccessStatusCode();

//            var jsonResponse = await createdVectorStore.Content.ReadAsStringAsync();
//            var storeId = JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();


//            if (documents.Count == 0 || !reqId.HasValue)
//            {
//                return;
//            }

//            var caseId = documents[0].CaseId;

//            //Upload File to Storage on Azure
//            List<(string FileId, string? DocName)> uploadedFiles = await UploadFileAsync(documents);
//            var fileListText = string.Join(Environment.NewLine,
//            uploadedFiles.Select(f => $"- {f.DocName} (file_id: {f.FileId})"));


//            //Upload the File Ids of the uploaded files to Vector Store
//            var fileIds = uploadedFiles.Select(f => f.FileId).ToList();
//            var jsonPayload = new
//            {
//                file_ids = fileIds
//            };

//            var jsonUploadFilesToVector = new StringContent(JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");

//            var uploadFileToVector = await _httpClient.PostAsync($"{_apiBaseUrl}/vector_stores/{storeId}/file_batches", jsonUploadFilesToVector);
//            uploadFileToVector.EnsureSuccessStatusCode();
//            var jsonResponseUploadedFiles = await uploadFileToVector.Content.ReadAsStringAsync();



//            //var promptText = "\"You are an expert Court Clerk or Court Judicial Assistant tasked with analyzing a debt collection case packet. Your objective is to determine whether the case should remain in the litigation (court) system or be transferred to a Credit Card Diversion Program for quicker resolution. This decision must be based on the facts and evidence presented in the case packet.\\r\\n\\r\\nCarefully review the case packet and evaluate the following criteria. Provide a detailed analysis with clear reasoning. Ensure all checks are thorough and aligned with the required legal standards.\\r\\n\\r\\n **Citations:** When citing, always use the document name which is passed in the input_text from user. No indexes:\\r\\nExample- 【DocumentName.extension, page. 1】.  \\r\\n\\r\\n\\r\\nNOTE: Please do not show the internal index number, show the exact page number of the document. \\r\\n\\r\\nEvaluation Criteria\\r\\n\\t1. Is this related to personal credit card debt collection?\\r\\n\\t\\t○ Verify if the debt pertains to a personal credit card (not a business credit card).\\r\\n\\t\\t○ Confirm if the defendant is an individual. Provide proof from the case documents.\\r\\n\\t2. What is the claim amount or debt collection amount?\\r\\n\\t\\t○ Identify the claim amount explicitly stated in the case packet (e.g., complaint or claim summary).\\r\\n\\t\\t○ Do not use values from unrelated sections (e.g., Final Statement Balance in an exhibit) unless the claim amount is clearly identified as such in the documentation.\\r\\n\\t\\t○ If the claim amount is not provided, indicate it as \\\"Not Provided\\\" and explain that it cannot be verified based on the available information.\\r\\n\\t3. Does the packet contain proof of credit card debt?\\r\\n\\t\\t○ Look for evidence of the debt, such as: \\r\\n\\t\\t\\t§ Itemized credit card statements.\\r\\n\\t\\t\\t§ Copies of the credit card agreement or contract.\\r\\n\\t\\t\\t§ Records of transactions leading to the claimed amount.\\r\\n\\t4. Does the packet contain information about the statute of limitation?\\r\\n\\t\\t○ Confirm if the debt is within the statute of limitations based on the last payment date or default date.\\r\\n\\t5. Has the debt been sold to third parties?\\r\\n\\t\\t○ If the debt was sold, ensure the packet includes: \\r\\n\\t\\t\\t§ A complete chain of title showing ownership transfers from the original creditor to the plaintiff.\\r\\n\\t\\t\\t§ Clear proof tying the debt to the defendant.\\r\\n\\t\\t○ Look for 100% proof of ownership to establish the plaintiff's legal right to collect the debt.\\r\\n\\t6. Does the Demonstrative Exhibit meet the 51% preponderance of evidence standard?\\r\\n\\t\\t○ Key Financial Details: List and review the following: \\r\\n\\t\\t\\t§ Final Statement Balance (if provided).\\r\\n\\t\\t\\t§ Total Purchases (sum of all itemized purchases).\\r\\n\\t\\t\\t§ Itemized Purchases (break down by date and amount).\\r\\n\\t\\t\\t§ Total Balance Transfers.\\r\\n\\t\\t\\t§ Total Cash Advances.\\r\\n\\t\\t\\t§ Total Deferred Interest Accrual.\\r\\n\\t\\t○ Exhibit Review: \\r\\n\\t\\t\\t§ Validate itemized purchases, balance transfers, cash advances.\\r\\n\\t\\t\\t§ Ensure these amounts are substantiated with supporting documentation.\\r\\n\\t\\t○ 51% Verification: \\r\\n\\t\\t\\t§ Calculate if at least 51% of the claim amount (if explicitly provided) is substantiated.\\r\\n\\t\\t\\t§ If the claim amount is not provided, state clearly that the 51% check cannot be performed due to insufficient data.\\r\\n\\t\\t\\t§ Include: \\r\\n\\t\\t\\t\\t□ Final Statement Balance: [Amount, if provided].\\r\\n\\t\\t\\t\\t□ Substantiated Amount: [Amount].\\r\\n\\t\\t\\t\\t□ Percentage Substantiated: [Percentage].\\r\\n\\t\\t○ Conclusion: \\r\\n\\t\\t\\t§ If 51% or more is substantiated, the claim is valid.\\r\\n\\t\\t\\t§ If less than 51% is substantiated, the claim is inadequately supported.\\r\\n\\t7. Has the complaint been properly served to the defendant?\\r\\n\\t\\t○ Review the Sheriff’s Return of Service or equivalent document for details.\\r\\n\\t\\t○ Look for clear indications of: \\r\\n\\t\\t\\t§ Successful service: \\\"Served the Complaint in Civil Action.\\\"\\r\\n\\t\\t\\t§ Unsuccessful service: \\\"Not Found\\\" or other relevant notes.\\r\\n\\t\\t○ Assess any deputy notes about attempts to serve, such as vacant properties or alternative service arrangements.\\r\\nException: For Notice of Appeal cases, service by mail is acceptable, and a Sheriff’s Return may not be required.\\r\\n\\r\\nFormatting Your Analysis\\r\\nFor each criterion, use the following format to provide a detailed and structured response:\\r\\n<analysis>\\r\\n1. Personal credit card debt and individual defendant:  \\r\\n[Detailed reasoning and evidence]\\r\\n2. Letter of notice:  \\r\\n[Detailed reasoning and evidence]\\r\\n3. Claim amount:  \\r\\n[Detailed reasoning and evidence]\\r\\n4. Proof of credit card debt:  \\r\\n[Detailed reasoning and evidence]\\r\\n5. Statute of limitation:  \\r\\n[Detailed reasoning and evidence]\\r\\n6. Debt ownership and chain of collectors:  \\r\\n[Detailed reasoning and evidence]\\r\\n7. Demonstrative Exhibit and 51% proof check:  \\r\\n[Detailed reasoning and evidence, including calculations if necessary]\\r\\n8. Service of the complaint:  \\r\\n[Detailed reasoning and evidence]\\r\\n</analysis>\\r\\n\\r\\nDecision Format\\r\\nAfter completing your analysis, decide whether the case is eligible for the Credit Card Diversion Program or should remain in the Litigation System. Use the following format for your decision:\\r\\n<decision>\\r\\nDecision: [Credit Card Diversion Program]  \\r\\nExplanation: [Summarize your reasoning, highlighting key criteria that influenced your decision. Clearly address any missing or insufficient evidence.]\\r\\n</decision>\\r\\n\\r\\n**Do not source or access any data outside the files provided here in vector store!**\\r\\n\"";
//            var input = new[]
//            {
//                new {
//                    role = "system",
//                    content = _promptText
//                }
//            };

//            // 3. Build request body for Responses API
//            var payload = new
//            {
//                model = _modelName,
//                input = input,
//                tools = new[]
//                {
//                    new
//                    {
//                        type = "file_search",
//                        vector_store_ids = new[] { storeId }
//                    }
//                },
//                max_output_tokens = 120000
//                //temperature = 0.0
//                //reasoning_effort = "low"
//            };

//            var jsonResponseAPI = JsonSerializer.Serialize(payload);
//            var contentHttp = new StringContent(jsonResponseAPI, Encoding.UTF8, "application/json");
//            string rawContent = string.Empty;

//            try
//            {
//                //var response = await _httpClient.PostAsync($"{_apiBaseUrl}/responses?api-version=2025-04-01-preview", contentHttp);
//                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/responses", contentHttp);
//                response.EnsureSuccessStatusCode();
//                rawContent = await response.Content.ReadAsStringAsync();
//            }
//            catch (HttpRequestException ex)
//            {
//                _logger.LogError(ex, "HTTP request failed when calling Responses API.");
//                await _httpClient.DeleteAsync($"{_apiBaseUrl}/vector_stores/{storeId}");
//                await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());
//                throw;
//            }
//            catch (TaskCanceledException ex)
//            {
//                _logger.LogError(ex, "Timeout calling Responses API.");
//                await _httpClient.DeleteAsync($"{_apiBaseUrl}/vector_stores/{storeId}");
//                await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());
//                throw;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error calling Responses API.");
//                await _httpClient.DeleteAsync($"{_apiBaseUrl}/vector_stores/{storeId}");
//                await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());
//                throw;
//            }


//            using var doc = JsonDocument.Parse(rawContent);
//            var root = doc.RootElement;

//            // Extract values
//            var id = root.GetProperty("id").GetString();
//            var status = root.GetProperty("status").GetString();

//            // Navigate into output → [0] → content → [0] → text
//            string? content = null;

//            try
//            {
//                if (root.TryGetProperty("output", out var outputProp) && outputProp.ValueKind == JsonValueKind.Array)
//                {
//                    foreach (var outputItem in outputProp.EnumerateArray())
//                    {
//                        if (outputItem.TryGetProperty("type", out var typeProp) &&
//                            typeProp.GetString() == "message" &&
//                            outputItem.TryGetProperty("content", out var contentArray) &&
//                            contentArray.ValueKind == JsonValueKind.Array &&
//                            contentArray.GetArrayLength() > 0)
//                        {
//                            var firstContent = contentArray[0];
//                            if (firstContent.TryGetProperty("text", out var textProp))
//                            {
//                                content = textProp.GetString();
//                                break; // found it, exit loop
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while extracting content from response JSON. Raw: {Raw}", rawContent);
//            }

//            _logger.LogDebug("Raw OpenAI API response content: {RawContent}", rawContent);


//            try
//            {
//                var getStatusFromAppDomainValue = _context!.AppDomainValues.Where(adv => adv.Code == status).Select(adv => adv.Code).FirstOrDefault()!;
//                var existingEntity = await _context!.ResponsesApiRequests.FirstOrDefaultAsync(r => r.ReqId == reqId);
//                if (existingEntity != null)
//                {
//                    existingEntity.CaseId = caseId;
//                    existingEntity.PromptId = _responseAPIPromptId;
//                    existingEntity.PromptVersion = id; // update to response id if needed
//                    existingEntity.Response = content;
//                    existingEntity.EligibilityCheckStatus = getStatusFromAppDomainValue;
//                    existingEntity.ModifiedUser = _sessionUser ?? "SYSTEM";
//                    existingEntity.ModifiedDttm = DateTime.UtcNow;
//                    existingEntity.IsChecked = true;
//                };

//                await _context.SaveChangesAsync();
//            }
//            catch
//            {
//                // Update existing record to failed
//                var existingEntity = await _context!.ResponsesApiRequests
//                    .FirstOrDefaultAsync(r => r.ReqId == reqId);

//                if (existingEntity != null)
//                {
//                    existingEntity.EligibilityCheckStatus = "failed";
//                    existingEntity.ModifiedUser = _sessionUser ?? "SYSTEM";
//                    existingEntity.ModifiedDttm = DateTime.UtcNow;
//                    existingEntity.IsChecked = true;

//                    await _context.SaveChangesAsync();
//                }
//            }

//            await _httpClient.DeleteAsync($"{_apiBaseUrl}/vector_stores/{storeId}");
//            await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());


//            return;

//        }


//        private async Task<List<(string FileId, string? DocName)>> UploadFileAsync(List<CaseDocumentViewModel> documents)
//        {
//            //var fileIds = new List<string>();
//            var uploadedFiles = new List<(string FileId, string? DocName)>();
//            foreach (var document in documents)
//            {

//                //MemoryStream textContentFromDocId = ;
//                byte[] fileContent = Array.Empty<byte>();
//                var docName = "";

//                if (_context != null)
//                {
//                    fileContent = _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.Content).FirstOrDefault()!;
//                    docName = _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.Name).FirstOrDefault();
//                }

//                using var memoryStream = new MemoryStream(fileContent ?? Array.Empty<byte>());

//                using var form = new MultipartFormDataContent();
//                form.Add(new StringContent("assistants"), "purpose");
//                var streamContent = new StreamContent(memoryStream);
//                form.Add(streamContent, "file", Path.GetFileName(docName ?? "upload"));

//                var uploaded = await _httpClient.PostAsync($"{_apiBaseUrl}/files", form);

//                uploaded.EnsureSuccessStatusCode();

//                var jsonResponse = await uploaded.Content.ReadAsStringAsync();
//                using var doc = JsonDocument.Parse(jsonResponse);
//                string fileId = doc.RootElement.GetProperty("id").GetString() ?? throw new Exception("No file id returned");

//                uploadedFiles.Add((fileId, Path.GetFileName(docName ?? string.Empty)));


//            }

//            return uploadedFiles;

//        }

//        private async Task DeleteUploadedFileAsync(List<string> fileIds)
//        {
//            if (fileIds == null || fileIds.Count == 0)
//                return;
//            try
//            {

//                foreach (var fileId in fileIds)
//                {
//                    try
//                    {
//                        //var deletionResult = await fileClient.DeleteFileAsync(fileId);
//                        var deletionResult = await _httpClient.DeleteAsync($"{_apiBaseUrl}/files/{fileId}");

//                        if (!deletionResult.IsSuccessStatusCode)
//                        {
//                            _logger.LogWarning("File deletion not confirmed for {FileId}", fileId);
//                        }
//                        else
//                        {
//                            _logger.LogInformation("Successfully deleted file {FileId}", fileId);
//                        }
//                    }
//                    catch (Exception exInner)
//                    {
//                        _logger.LogError(exInner, "Error deleting file {FileId}", fileId);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error initializing OpenAI client for file deletion.");
//            }
//        }

//    }
//}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Syncfusion.EJ2.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.ClientModel;
using OpenAI;
using System.ClientModel.Primitives;
using LancasterCreditCardDiversion.ViewModels;
using LancasterCreditCardDiversion.Models;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles Open AI Services
    /// </summary>
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly string _apiBaseUrl;
        private readonly string _modelName;
        private readonly CommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;
        private readonly PaLancCcdpDevDbContext? _context;
        private readonly ILogger<TimeHostedCheckEligibilityService> _logger;
        private readonly string _responseAPIPromptId;
        private readonly string _promptText;

        public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<OpenAIConfigViewModel> options, CommonService commonService, IHttpContextAccessor httpContextAccessor, PaLancCcdpDevDbContext context, ILogger<TimeHostedCheckEligibilityService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = options.Value.ApiKey;
            _apiBaseUrl = options.Value.ApiBaseUrl;
            _modelName = options.Value.ModelName;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            _context = context;
            _logger = logger;
            _responseAPIPromptId = configuration["ResponseAPIPromptId"] ?? throw new ArgumentNullException("ResponseAPIPromptId configuration is missing");
            _promptText = Prompt.EligibilityPrompt;
        }
        public async Task CheckDocumentEligibility(List<CaseDocumentViewModel> documents, decimal? reqId)
        {
            if (documents.Count == 0 || !reqId.HasValue)
            {
                return;
            }

            var caseId = documents[0].CaseId;

            List<(string FileId, string? DocName)> uploadedFiles = await UploadFileAsync(documents);
            var fileListText = string.Join(Environment.NewLine,
            uploadedFiles.Select(f => $"- {f.DocName} (file_id: {f.FileId})"));

            var promptText = $@"
            You are an expert Court Clerk or Court Judicial Assistant tasked with analyzing a debt collection case packet files.

            Files you have access to:
            {fileListText}

            Now please analyze these {uploadedFiles.Count} document(s).";

            var payload = CreateRequestPayloadMulti(_modelName, uploadedFiles!, promptText);

            // 1. Create a SocketsHttpHandler for Keep-Alive and long timeout
            var socketsHandler = new SocketsHttpHandler
            {
                // Configure the connection pool to allow long-running connections
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),

                // Send a ping every 15s to keep the connection open and bypass network timeouts
                KeepAlivePingDelay = TimeSpan.FromSeconds(15),
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                KeepAlivePingTimeout = TimeSpan.FromSeconds(5)
            };


            // 2. Create a custom HttpClient instance from the handler
            var customHttpClient = new HttpClient(socketsHandler)
            {
                // The overall timeout for the *entire* operation
                Timeout = TimeSpan.FromMinutes(30)
            };

            // 3. Create a custom transport using the configured HttpClient
            var customTransport = new HttpClientPipelineTransport(customHttpClient);

            // 4. Create the options object, injecting the custom transport
            var clientOptions = new OpenAIClientOptions()
            {
                Transport = customTransport,
                NetworkTimeout = TimeSpan.FromMinutes(10)
            };

            //var responseClient = new OpenAIResponseClient(_deploymentName, new ApiKeyCredential(_apiKey!), clientOptions);
            var parentClient = new OpenAIClient(new ApiKeyCredential(_apiKey!), clientOptions);

            // 3. Get the sub-client using the factory method. 
            // This correctly passes the configured Pipeline and Options down to the sub-client.
            var responseClient = parentClient.GetOpenAIResponseClient(_modelName);

            try
            {

                var response = await responseClient.CreateResponseAsync(content: BinaryContent.Create(BinaryData.FromObjectAsJson(payload)));


                if (response.GetRawResponse().Status != 200)
                {
                    _logger.LogError("OpenAI API call failed with status: {Status}", response.GetRawResponse().Status);
                }

                var rawContent = response.GetRawResponse().Content.ToString();

                using var doc = JsonDocument.Parse(rawContent);
                var root = doc.RootElement;

                // Extract values
                var id = root.GetProperty("id").GetString();
                var status = root.GetProperty("status").GetString();

                if (status == "incomplete")
                {
                    _logger.LogWarning("Responses API returned status 'incomplete'. Attempting to fetch remaining chunks...");
                }
                // Navigate into output → [0] → content → [0] → text
                string? content = null;

                if (root.TryGetProperty("output", out var outputProp) && outputProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var outputItem in outputProp.EnumerateArray())
                    {
                        if (outputItem.TryGetProperty("type", out var typeProp) &&
                            typeProp.GetString() == "message" &&
                            outputItem.TryGetProperty("content", out var contentArray) &&
                            contentArray.ValueKind == JsonValueKind.Array &&
                            contentArray.GetArrayLength() > 0)
                        {
                            var firstContent = contentArray[0];
                            if (firstContent.TryGetProperty("text", out var textProp))
                            {
                                content = textProp.GetString();
                                break; // found it, exit loop
                            }
                        }
                    }
                }

                try
                {
                    var getStatusFromAppDomainValue = _context!.AppDomainValues.Where(adv => adv.Code == status).Select(adv => adv.Code).FirstOrDefault()!;
                    if (getStatusFromAppDomainValue == "failed" && response.GetRawResponse().Status == 200)
                    {
                        getStatusFromAppDomainValue = "incomplete";
                    }
                    var existingEntity = await _context!.ResponsesApiRequests.FirstOrDefaultAsync(r => r.ReqId == reqId);
                    if (existingEntity != null)
                    {
                        existingEntity.CaseId = caseId;
                        existingEntity.PromptId = _responseAPIPromptId;
                        existingEntity.PromptVersion = id; // update to response id if needed
                        existingEntity.Response = content;
                        existingEntity.EligibilityCheckStatus = getStatusFromAppDomainValue;
                        existingEntity.ModifiedUser = _sessionUser ?? "SYSTEM";
                        existingEntity.ModifiedDttm = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                        existingEntity.IsChecked = true;
                    };

                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Update existing record to failed
                    var existingEntity = await _context!.ResponsesApiRequests
                        .FirstOrDefaultAsync(r => r.ReqId == reqId);

                    if (existingEntity != null)
                    {
                        existingEntity.IsChecked = true;

                        await _context.SaveChangesAsync();
                    }
                }


            }
            catch (Exception ex)
            {
                // Unexpected error — maybe network, deserialization, etc.
                _logger.LogError(ex, "Unexpected error calling OpenAI.");
                // Optionally retry depending on type
            }

            await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());

            return;

        }


        private object CreateRequestPayloadMulti(string modelOrDeployment, List<(string FileId, string DocName)> uploadedFiles, string userPrompt)
        {
            var fileItems = uploadedFiles.Select(doc => (object)new
            {
                type = "input_file",
                file_id = doc.FileId
            }).ToList();

            // Add the user's actual prompt
            fileItems.Add(new { type = "input_text", text = userPrompt });

            return new
            {
                model = modelOrDeployment,
                prompt = new
                {
                    id = _responseAPIPromptId
                },

                input = new object[]
                {
                    new
                    {
                        role = "user",
                        content = fileItems.ToArray()
                    }
                },

                text = new
                {
                    format = new { type = "text" }
                },

                reasoning = new
                {
                    effort = "low"
                },
                max_output_tokens = 120000
            };
        }


        private async Task<List<(string FileId, string? DocName)>> UploadFileAsync(List<CaseDocumentViewModel> documents)
        {
            //var fileIds = new List<string>();
            var uploadedFiles = new List<(string FileId, string? DocName)>();
            foreach (var document in documents)
            {

                //MemoryStream textContentFromDocId = ;
                byte[] fileContent = Array.Empty<byte>();
                var docName = "";

                if (_context != null)
                {
                    fileContent = _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.Content).FirstOrDefault()!;
                    docName = _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.Name).FirstOrDefault();
                }

                //using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(textContentFromDocId!));
                var openAI = new OpenAI.OpenAIClient(_apiKey);
                var fileClient = openAI.GetOpenAIFileClient();
                var baseName = Path.GetFileNameWithoutExtension(docName);
                var ext = Path.GetExtension(docName!).ToLowerInvariant();

                MemoryStream pdfStream;

                if (ext == ".pdf")
                {
                    pdfStream = new MemoryStream(fileContent!);
                }
                else
                {
                    var converter = new ConvertAnyFileToPdf();
                    pdfStream = await converter.ConvertToPdfAsync(fileContent, docName!);
                }
                var uploaded = await fileClient.UploadFileAsync(pdfStream, baseName + ".pdf", "user_data");

                if (uploaded.Value.Id != null)
                {
                    uploadedFiles.Add((uploaded.Value.Id, Path.GetFileName(docName)));
                }
                else
                {
                    throw new Exception("File upload failed.");
                }
            }

            return uploadedFiles;

        }

        private async Task DeleteUploadedFileAsync(List<string> fileIds)
        {
            if (fileIds == null || fileIds.Count == 0)
                return;
            try
            {
                var openAI = new OpenAI.OpenAIClient(_apiKey);
                var fileClient = openAI.GetOpenAIFileClient();
                foreach (var fileId in fileIds)
                {
                    try
                    {
                        var deletionResult = await fileClient.DeleteFileAsync(fileId);

                        if (deletionResult?.Value?.Deleted != true)
                        {
                            _logger.LogWarning("File deletion not confirmed for {FileId}", fileId);
                        }
                        else
                        {
                            _logger.LogInformation("Successfully deleted file {FileId}", fileId);
                        }
                    }
                    catch (Exception exInner)
                    {
                        _logger.LogError(exInner, "Error deleting file {FileId}", fileId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OpenAI client for file deletion.");
            }
        }

    }
}
