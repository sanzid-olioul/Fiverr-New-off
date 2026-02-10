//using Azure;
//using Azure.AI.DocumentIntelligence;
//using LancasterCreditCardDiversion.Models;
//using LancasterCreditCardDiversion.Services;
//using LancasterCreditCardDiversion.ViewModels;
//using Microsoft.CodeAnalysis.Operations;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Syncfusion.EJ2.Linq;
//using Syncfusion.OCRProcessor;
//using Syncfusion.Pdf;
//using Syncfusion.Pdf.Graphics;
//using Syncfusion.Pdf.Parsing;
//using Syncfusion.PdfToImageConverter;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using JsonException = System.Text.Json.JsonException;
//using JsonSerializer = System.Text.Json.JsonSerializer;
//using Languages = Syncfusion.OCRProcessor.Languages;

//namespace LancasterCreditCardDiversion.Services
//{
//    /// <summary>
//    /// Handles Open AI Services
//    /// </summary>
//    public class OpenAIService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string? _apiKey;
//        private readonly string? _deploymentName;
//        private readonly string _apiBaseUrl;
//        private readonly CommonService _commonService;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly string? _sessionUser;
//        private readonly PaLancCcdpDevDbContext? _context;
//        private readonly DocumentIntelligenceClient _client;
//        private readonly string? _assistantId;

//        public OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<SmartComponentsViewModel> options, CommonService commonService, IHttpContextAccessor httpContextAccessor, PaLancCcdpDevDbContext context, DocumentIntelligenceClient client)
//        {
//            _httpClient = new HttpClient();
//            _apiKey = options.Value.ApiKey;
//            _deploymentName = options.Value.DeploymentName;
//            _apiBaseUrl = options.Value.ApiBaseUrl;
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
//            _commonService = commonService;
//            _httpContextAccessor = httpContextAccessor;
//            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
//            _context = context;
//            _client = client;
//            _assistantId = configuration["AssistantOpenAIId"];
//        }

//        #region 

//        public async Task CheckDocumentEligibility(List<CaseDocumentViewModel> documents, int? reqId)
//        {
//            if (documents.Count == 0 || !reqId.HasValue)
//            {
//                return;
//            }
//            //var prompt = "";
//            string? status = "";

//            // Step 1: Create an assistant (if needed)
//            // (Assuming assistant creation is done once, otherwise you can make this dynamic based on your requirements)
//            string assistantId = _assistantId ?? throw new Exception("AssistantOpenAIId is not configured properly.");

//            //Step 2: Check document word count
//            var (isSuccess, message) = await ProcessDocumentsAsync(documents);

//            if (isSuccess)
//            {
//                //Step 3: Create Vector Store
//                var vectorStoreId = await CreateVectorStoreAsync("Documents");

//                //Step 4: Upload file to Open AI 
//                List<string> fileIds = await UploadFileAsync(documents, "assistants");

//                //Step 5: Add fileIds to Vector Store
//                await StoreFilesInVectorStoreAsync(vectorStoreId, fileIds);

//                //Step 6: Update the assistant with the Vector Store Id
//                await UpdateAssistantAsync(assistantId, vectorStoreId);

//                // Step 7: Create a thread, save ThreadId and AssistantId in the database
//                string? threadId;

//                if (_context != null)
//                {
//                    var getQueuedRecord = await _context.EligibilityCheckRequests
//                        .Where(er => er.ReqId == reqId)
//                        .FirstOrDefaultAsync();

//                    if (getQueuedRecord != null && !string.IsNullOrWhiteSpace(getQueuedRecord.ThreadId) && getQueuedRecord.ThreadId != "temp")
//                    {
//                        threadId = getQueuedRecord.ThreadId; // Reuse existing thread
//                    }
//                    else
//                    {
//                        threadId = await CreateThread(fileIds);
//                        if (getQueuedRecord != null && threadId != null)
//                        {
//                            getQueuedRecord.ThreadId = threadId;
//                            getQueuedRecord.AssistantId = assistantId;
//                            await _context.SaveChangesAsync();
//                        }
//                    }
//                }
//                else
//                {
//                    threadId = await CreateThread(fileIds); // fallback, but should not happen
//                }


//                if (threadId != null)
//                {
//                    // Step 8: Run the assistant
//                    var runId = await RunAssistant(threadId, assistantId);

//                    // Step 9: Check the run status and get response
//                    int attempts = 0;
//                    const int maxAttempts = 500; // Maximum attempts (e.g., 30 seconds)
//                    var eligibility_status_from_db = "";
//                    if (_context != null)
//                    {
//                        eligibility_status_from_db = await _context.EligibilityCheckRequests.Where(r => r.ThreadId == threadId).Select(r => r.EligibilityCheckStatus).FirstOrDefaultAsync();
//                    }

//                    if (eligibility_status_from_db != "failed")
//                    {
//                        do
//                        {
//                            status = await CheckRunStatus(threadId, runId);
//                            attempts++;
//                            if (status == "completed")
//                            {
//                                // Step 6: Retrieve the assistant's response
//                                await Task.Delay(1000);
//                                await GetAssistantResponse(threadId);
//                                if (_context != null)
//                                {
//                                    var getStatusFromAppDomainValue = _context.AppDomainValues.Where(adv => adv.Code == status).Select(adv => adv.Code).FirstOrDefault();
//                                    var requestRecordBasedOnTreadId = await _context.EligibilityCheckRequests.Where(r => r.ThreadId == threadId).FirstOrDefaultAsync();
//                                    if (requestRecordBasedOnTreadId != null && getStatusFromAppDomainValue != null)
//                                    {
//                                        var checkResponse = await _context.EligibilityCheckRequests.Where(er => er.ThreadId == threadId).Select(er => er.Response).FirstOrDefaultAsync();
//                                        if (checkResponse != null)
//                                        {
//                                            requestRecordBasedOnTreadId.EligibilityCheckStatus = getStatusFromAppDomainValue;
//                                            await _context.SaveChangesAsync();
//                                        }
//                                        else
//                                        {
//                                            requestRecordBasedOnTreadId.EligibilityCheckStatus = "failed";
//                                            await _context.SaveChangesAsync();
//                                        }
//                                    }
//                                }

//                            }
//                            else
//                            {
//                                if (_context != null)
//                                {
//                                    var getStatusFromAppDomainValue = _context.AppDomainValues.Where(adv => adv.Code == status).Select(adv => adv.Code).FirstOrDefault();
//                                    var requestRecordBasedOnTreadId = await _context.EligibilityCheckRequests.Where(r => r.ThreadId == threadId).FirstOrDefaultAsync();

//                                    if (requestRecordBasedOnTreadId != null && getStatusFromAppDomainValue != null)
//                                    {
//                                        requestRecordBasedOnTreadId.EligibilityCheckStatus = getStatusFromAppDomainValue;
//                                        await _context.SaveChangesAsync();
//                                    }
//                                }
//                            }
//                        } while (status != "completed" && attempts < maxAttempts);
//                    }


//                    if (attempts > maxAttempts)
//                    {
//                        if (_context != null)
//                        {
//                            var requestRecordBasedOnTreadId = await _context.EligibilityCheckRequests.Where(r => r.ThreadId == threadId).FirstOrDefaultAsync();

//                            if (requestRecordBasedOnTreadId != null)
//                            {
//                                requestRecordBasedOnTreadId.EligibilityCheckStatus = "expired";
//                                await _context.SaveChangesAsync();
//                            }
//                        }
//                    }

//                    //Step 10: Delete uploaded files and vector store
//                    await DeleteUploadedFileFromOpenAI(fileIds);
//                    await DeleteVectorStore(vectorStoreId);

//                }
//            }

//            return;
//        }

//        private async Task<string?> CreateVectorStoreAsync(string storeName)
//        {
//            var requestBody = new { name = storeName };
//            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/vector_stores", jsonContent);
//            response.EnsureSuccessStatusCode();

//            var jsonResponse = await response.Content.ReadAsStringAsync();
//            return JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();
//        }

//        public async Task<List<string>> UploadFileAsync(List<CaseDocumentViewModel> documents, string purpose)
//        {
//            //await Task.Delay(TimeSpan.FromSeconds(10));
//            var fileIds = new List<string>();
//            foreach (var document in documents)
//            {
//                var textContentFromDocId = "";

//                if (_context != null)
//                {
//                    textContentFromDocId = _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.TextContent).FirstOrDefault();
//                }

//                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(textContentFromDocId!));
//                using var form = new MultipartFormDataContent
//                {
//                    { new StringContent(purpose), "purpose" },
//                    { new StreamContent(memoryStream), "file", document.Name }
//                };

//                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/files", form);
//                response.EnsureSuccessStatusCode();

//                var jsonResponse = await response.Content.ReadAsStringAsync();
//                // Parse the response to get the file ID directly from JSON
//                using var jsonDoc = JsonDocument.Parse(jsonResponse);
//                var fileId = jsonDoc.RootElement.GetProperty("id").GetString();

//                if (fileId != null)
//                {
//                    fileIds.Add(fileId); // Add the uploaded file ID to the list
//                }
//                //File.Delete(tempFilePath);
//            }
//            return fileIds;
//        }

//        public async Task StoreFilesInVectorStoreAsync(string? vectorStoreId, List<string> fileIds)
//        {
//            // Create the JSON payload
//            var jsonPayload = new
//            {
//                file_ids = fileIds
//            };

//            var jsonContent = new StringContent(JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");

//            // Send the POST request to the vector store
//            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/vector_stores/{vectorStoreId}/file_batches", jsonContent);

//            // Ensure the response indicates success
//            response.EnsureSuccessStatusCode();

//            // Parse the response if needed
//            var jsonResponse = await response.Content.ReadAsStringAsync();

//        }


//        private async Task UpdateAssistantAsync(string assistantId, string? vectorStoreId)
//        {
//            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/assistants/{assistantId}");
//            response.EnsureSuccessStatusCode();

//            var jsonResponse = await response.Content.ReadAsStringAsync();
//            var assistantConfig = JsonDocument.Parse(jsonResponse);

//            // Step 2: Prepare the Updated Request Body
//            var updatedRequestBody = new
//            {
//                tool_resources = new
//                {
//                    file_search = new
//                    {
//                        vector_store_ids = new[] { vectorStoreId }
//                    }
//                },
//            };

//            // Serialize the updated request body to JSON
//            var jsonContent = new StringContent(JsonSerializer.Serialize(updatedRequestBody), Encoding.UTF8, "application/json");

//            var responseUpdated = await _httpClient.PostAsync($"{_apiBaseUrl}/assistants/{assistantId}", jsonContent);
//            response.EnsureSuccessStatusCode();

//            var jsonResponseUpdated = await response.Content.ReadAsStringAsync();
//        }

//        private async Task<string?> CreateThread(List<string> fileIds /* string? vectorStoreId*/)
//        {
//            var requestBody = new
//            {
//                messages = new[]
//              {
//                new
//                {
//                    role = "user",
//                    content = "Please analyze the below case packet and give answer for each file. Please include file citations in your analysis.",
//                }
//            }
//            };
//            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/threads", jsonContent);
//            response.EnsureSuccessStatusCode();

//            var jsonResponse = await response.Content.ReadAsStringAsync();

//            await Task.Delay(2000);
//            return JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();
//        }

//        private async Task<string?> RunAssistant(string? threadId, string assistantId)
//        {
//            try
//            {
//                var requestBody = new
//                {
//                    assistant_id = assistantId, // Include the assistant ID
//                };

//                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
//                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/threads/{threadId}/runs", jsonContent);
//                response.EnsureSuccessStatusCode();

//                var jsonResponse = await response.Content.ReadAsStringAsync();
//                return JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"OpenAI run failed for thread {threadId}: {ex.Message}");

//                // Mark the thread as failed in DB
//                if (!string.IsNullOrEmpty(threadId) && _context != null)
//                {
//                    // Get all records with this threadId, ordered by CreatedDttm descending
//                    var records = await _context.EligibilityCheckRequests
//                        .Where(r => r.ThreadId == threadId)
//                        .OrderByDescending(r => r.CreatedDttm)
//                        .ToListAsync();

//                    // Get the newest record (first one)
//                    var latestRecord = records.FirstOrDefault();

//                    // Now mark all others as failed
//                    foreach (var record in records)
//                    {
//                        record.EligibilityCheckStatus = "failed";
//                        record.ModifiedUser = _sessionUser ?? "system";
//                        record.ModifiedDttm = DateTime.UtcNow;
//                    }

//                    try
//                    {
//                        await _context.SaveChangesAsync();
//                    }
//                    catch (Exception dbEx)
//                    {
//                        Console.WriteLine($"Failed to update DB: {dbEx.Message}");
//                    }
//                }

//                return null;

//            }

//        }

//        private async Task<string?> CheckRunStatus(string? threadId, string? runId)
//        {
//            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/threads/{threadId}/runs/{runId}");
//            response.EnsureSuccessStatusCode();

//            var jsonResponse = await response.Content.ReadAsStringAsync();
//            return JsonDocument.Parse(jsonResponse).RootElement.GetProperty("status").GetString();
//        }


//        private async Task<string?> GetAssistantResponse(string? threadId)
//        {
//            // Ensure threadId is not null or empty
//            if (string.IsNullOrEmpty(threadId))
//            {
//                throw new ArgumentException("Thread ID cannot be null or empty.", nameof(threadId));
//            }

//            await Task.Delay(6000);

//            //const int maxRetries = 3; // Maximum number of retry attempts
//            //int retryCount = 0;
//            try
//            {
//                var messageResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/threads/{threadId}/messages");
//                messageResponse.EnsureSuccessStatusCode();

//                var responseJson = await messageResponse.Content.ReadAsStringAsync();
//                var messages = JsonDocument.Parse(responseJson).RootElement.GetProperty("data");

//                string? latestResponse = null;

//                // Find the last assistant message
//                var assistantMessage = messages.EnumerateArray()
//                    .LastOrDefault(message => message.TryGetProperty("role", out var role) && role.GetString() == "assistant");

//                if (assistantMessage.ValueKind != JsonValueKind.Undefined)
//                {
//                    // Check if the "content" property exists and is an array
//                    if (assistantMessage.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
//                    {
//                        // Get the last message from the content array
//                        var lastContentMessage = content.EnumerateArray().LastOrDefault();

//                        if (lastContentMessage.ValueKind != JsonValueKind.Undefined &&
//                            lastContentMessage.TryGetProperty("text", out var textProperty) &&
//                            textProperty.ValueKind == JsonValueKind.Object)
//                        {
//                            // Get the "value" property
//                            if (textProperty.TryGetProperty("value", out var valueProperty))
//                            {
//                                latestResponse = valueProperty.GetString()?.Trim();
//                            }
//                        }
//                    }
//                }

//                // Update the database if the context is available
//                if (_context != null)
//                {
//                    var requestRecord = await _context.EligibilityCheckRequests
//                        .Where(r => r.ThreadId == threadId)
//                        .FirstOrDefaultAsync();

//                    if (requestRecord != null)
//                    {
//                        requestRecord.Response = latestResponse;

//                        try
//                        {
//                            await _context.SaveChangesAsync();
//                        }
//                        catch (DbUpdateException dbEx)
//                        {
//                            throw new Exception("Database update error: " + dbEx.Message, dbEx);
//                        }
//                        catch (Exception ex)
//                        {
//                            throw new Exception("Error saving changes: " + ex.Message, ex);
//                        }
//                    }
//                }

//                return latestResponse;
//            }
//            catch (HttpRequestException httpEx)
//            {
//                throw new Exception("Error fetching messages from the server: " + httpEx.Message, httpEx);
//            }
//            catch (JsonException jsonEx)
//            {
//                throw new Exception("Error parsing the response: " + jsonEx.Message, jsonEx);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
//            }
//        }


//        public async Task<(bool isSuccess, string message)> ProcessDocumentsAsync(List<CaseDocumentViewModel> documents)
//        {
//            var totalWordCount = 0;
//            int? getWordCount = 0;
//            var getTextContent = "";

//            foreach (var document in documents)
//            {
//                if (_context != null)
//                {
//                    getTextContent = await _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.TextContent).FirstOrDefaultAsync();
//                    getWordCount = (int?)await _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).Select(cd => cd.WordCount).FirstOrDefaultAsync();

//                    string textWords = await AnalyzeDocumentFromBytesAsync(document);
//                    var wordCount = textWords.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

//                    var requestRecord = await _context.CaseDocuments.Where(cd => cd.DocId == document.DocId).FirstOrDefaultAsync();

//                    if (requestRecord != null)
//                    {
//                        requestRecord.TextContent = textWords;
//                        requestRecord.WordCount = wordCount;
//                        await _context.SaveChangesAsync();
//                    }

//                    totalWordCount += wordCount;
//                }
//            }

//            if (_context != null)
//            {
//                var maxWordCount = await _context.AppParameters.Where(ap => ap.Name == "MAX_WORD_COUNT_AI").Select(ap => ap.Value).FirstOrDefaultAsync();
//                if (getWordCount > Convert.ToDecimal(maxWordCount) || totalWordCount > Convert.ToDecimal(maxWordCount))
//                {
//                    return (false, $"File too large, has more than 100K words.");
//                }
//            }

//            return (true, "Documents processed successfully.");
//        }

//        private async Task<string> AnalyzeDocumentFromBytesAsync(CaseDocumentViewModel document)
//        {
//            if (document.Content == null || document.Content.Length == 0)
//            {
//                throw new ArgumentException($"Document content is null or empty for DocId: {document.DocId}");
//            }

//            using (var memoryStream = new MemoryStream(document.Content))
//            {
//                try
//                {
//                    BinaryData binaryData = new BinaryData(document.Content);

//                    // Create content using the base64-encoded document
//                    var content = new AnalyzeDocumentContent
//                    {
//                        Base64Source = binaryData
//                    };

//                    // Perform document analysis using the "prebuilt-layout" model and output format as Markdown
//                    Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(
//                        WaitUntil.Completed,
//                        "prebuilt-layout",
//                        content,
//                        outputContentFormat: ContentFormat.Markdown
//                    );

//                    // Retrieve the result of the analysis
//                    AnalyzeResult result = operation.Value;

//                    string markdownContent = result.Content;

//                    var textWord = new StringBuilder();

//                    string[] pages = markdownContent.Split(new string[] { "\n\n---\n\n" }, StringSplitOptions.None);  // Adjust split pattern if necessary

//                    // Iterate through each page and append it to the result
//                    for (int i = 0; i < pages.Length; i++)
//                    {
//                        textWord.AppendLine($"--- Page {i + 1} ---"); // Add page separator marker (you can customize this)
//                        textWord.AppendLine(pages[i].Trim()); // Append the page content and trim any extra whitespace
//                        textWord.AppendLine("\n"); // Add extra space or new line after each page (can be customized)
//                    }

//                    return textWord.ToString(); // Return the concatenated text
//                }
//                catch (Exception ex)
//                {
//                    throw new Exception("Error saving changes: " + ex.Message, ex);

//                }

//            }
//        }

//        private string CleanTextToParagraphs(string extractedText)
//        {
//            string cleanText = System.Text.RegularExpressions.Regex.Replace(extractedText, @"\r\n|\r|\n", " ");

//            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"\s+", " ").Trim();

//            return cleanText;
//        }

//        private async Task DeleteUploadedFileFromOpenAI(List<string> fileIds)
//        {
//            foreach (var fileId in fileIds)
//            {
//                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/files/{fileId}");
//            }

//        }

//        private async Task DeleteVectorStore(string? vectorStoreId)
//        {
//            await _httpClient.DeleteAsync($"{_apiBaseUrl}/vector_stores/{vectorStoreId}");
//        }
//        #endregion


//    }
//}

//using Azure.AI.DocumentIntelligence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Syncfusion.EJ2.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using OpenAI.Responses;
//using System.ClientModel;
//using LancasterCreditCardDiversion.ViewModels;
//using LancasterCreditCardDiversion.Models;
//using Azure;
//using Azure.AI.OpenAI.Files;
//using Azure.AI.OpenAI;
//using OpenAI.VectorStores;
//using Org.BouncyCastle.Tls;
//// using OpenAI;
//// using OpenAI.Files;

//namespace LancasterCreditCardDiversion.Services
//{
//    /// <summary>
//    /// Handles Open AI Services
//    /// </summary>
//    public class OpenAIServicev
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string? _apiKey;
//        private readonly string _deploymentName;
//        private readonly string _apiBaseUrl;
//        private readonly string _modelName;
//        private readonly CommonService _commonService;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly string? _sessionUser;
//        private readonly PaLancCcdpDevDbContext? _context;
//        private readonly DocumentIntelligenceClient _client;
//        private readonly ILogger<TimeHostedCheckEligibilityService> _logger;
//        private readonly string _responseAPIPromptId;

//        public OpenAIServicev(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<OpenAIConfigViewModel> options, CommonService commonService, IHttpContextAccessor httpContextAccessor, PaLancCcdpDevDbContext context, DocumentIntelligenceClient client, ILogger<TimeHostedCheckEligibilityService> logger)
//        {
//            _httpClient = new HttpClient();
//            _apiKey = options.Value.ApiKey;
//            _deploymentName = options.Value.ModelName;
//            _apiBaseUrl = options.Value.ApiBaseUrl;
//            _modelName = options.Value.ModelName;
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            _commonService = commonService;
//            _httpContextAccessor = httpContextAccessor;
//            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
//            _context = context;
//            _client = client;
//            _logger = logger;
//            _responseAPIPromptId = configuration["ResponseAPIPromptId"] ?? throw new ArgumentNullException("ResponseAPIPromptId configuration is missing");
//        }

//        public async Task CheckDocumentEligibility(List<CaseDocumentViewModel> documents, decimal? reqId)
//        {
//            var endpoint = new Uri(_apiBaseUrl);
//            var apiKey = new AzureKeyCredential(_apiKey!);

//            AzureOpenAIClient azureClient = new(endpoint, apiKey);

//            if (documents.Count == 0 || !reqId.HasValue)
//            {
//                return;
//            }

//            var caseId = documents[0].CaseId;

//            //Create the Vector Store
//            var vectorStoreClient = azureClient.GetVectorStoreClient();
//            CreateVectorStoreOperation vectorStore = await vectorStoreClient.CreateVectorStoreAsync(true);
//            string storeId = vectorStore.VectorStoreId;


//            //Upload File to Storage on Azure
//            List<(string FileId, string? DocName)> uploadedFiles = await UploadFileAsync(documents);
//            var fileListText = string.Join(Environment.NewLine,
//            uploadedFiles.Select(f => $"- {f.DocName} (file_id: {f.FileId})"));


//            //Upload the File Ids of the uploaded files to Vector Store
//            foreach (var file in uploadedFiles)
//            {
//                AddFileToVectorStoreOperation addFile =
//                    await vectorStoreClient.AddFileToVectorStoreAsync(storeId, file.FileId, waitUntilCompleted: true);
//            }


//            var promptText = $@"
//            You are an expert Court Clerk or Court Judicial Assistant tasked with analyzing a debt collection case packet files.

//            Files you have access to:
//            {fileListText}

//            **When citing, always use the exact document name listed above (e.g., Complaint.pdf), not the file_id.**
//            Now please analyze these {uploadedFiles.Count} document(s).";

//            //var payload = CreateRequestPayloadMulti(_deploymentName, uploadedFiles!, promptText);
         
//            //var responseClient = new OpenAIResponseClient(_deploymentName, _apiKey);

//            // Build input items
//            var inputItems = new List<ResponseItem>
//            {
//                ResponseItem.CreateSystemMessageItem("You are an expert Court Clerk or Court Judicial Assistant tasked with analyzing a debt collection case packet. Your objective is to determine whether the case should remain in the litigation (court) system or be transferred to a Credit Card Diversion Program for quicker resolution. This decision must be based on the facts and evidence presented in the case packet.\r\n\r\nCarefully review the case packet and evaluate the following criteria. Provide a detailed analysis with clear reasoning. Ensure all checks are thorough and aligned with the required legal standards.\r\n\r\n **Citations:** When citing, always use the document name which is passed in the input_text from user. No indexes:\r\nExample- 【DocumentName.extension, page. 1】.  \r\n\r\n\r\nNOTE: Please do not show the internal index number, show the exact page number of the document. \r\n\r\nEvaluation Criteria\r\n\t1. Is this related to personal credit card debt collection?\r\n\t\t○ Verify if the debt pertains to a personal credit card (not a business credit card).\r\n\t\t○ Confirm if the defendant is an individual. Provide proof from the case documents.\r\n\t2. What is the claim amount or debt collection amount?\r\n\t\t○ Identify the claim amount explicitly stated in the case packet (e.g., complaint or claim summary).\r\n\t\t○ Do not use values from unrelated sections (e.g., Final Statement Balance in an exhibit) unless the claim amount is clearly identified as such in the documentation.\r\n\t\t○ If the claim amount is not provided, indicate it as \"Not Provided\" and explain that it cannot be verified based on the available information.\r\n\t3. Does the packet contain proof of credit card debt?\r\n\t\t○ Look for evidence of the debt, such as: \r\n\t\t\t§ Itemized credit card statements.\r\n\t\t\t§ Copies of the credit card agreement or contract.\r\n\t\t\t§ Records of transactions leading to the claimed amount.\r\n\t4. Does the packet contain information about the statute of limitation?\r\n\t\t○ Confirm if the debt is within the statute of limitations based on the last payment date or default date.\r\n\t5. Has the debt been sold to third parties?\r\n\t\t○ If the debt was sold, ensure the packet includes: \r\n\t\t\t§ A complete chain of title showing ownership transfers from the original creditor to the plaintiff.\r\n\t\t\t§ Clear proof tying the debt to the defendant.\r\n\t\t○ Look for 100% proof of ownership to establish the plaintiff's legal right to collect the debt.\r\n\t6. Does the Demonstrative Exhibit meet the 51% preponderance of evidence standard?\r\n\t\t○ Key Financial Details: List and review the following: \r\n\t\t\t§ Final Statement Balance (if provided).\r\n\t\t\t§ Total Purchases (sum of all itemized purchases).\r\n\t\t\t§ Itemized Purchases (break down by date and amount).\r\n\t\t\t§ Total Balance Transfers.\r\n\t\t\t§ Total Cash Advances.\r\n\t\t\t§ Total Deferred Interest Accrual.\r\n\t\t○ Exhibit Review: \r\n\t\t\t§ Validate itemized purchases, balance transfers, cash advances.\r\n\t\t\t§ Ensure these amounts are substantiated with supporting documentation.\r\n\t\t○ 51% Verification: \r\n\t\t\t§ Calculate if at least 51% of the claim amount (if explicitly provided) is substantiated.\r\n\t\t\t§ If the claim amount is not provided, state clearly that the 51% check cannot be performed due to insufficient data.\r\n\t\t\t§ Include: \r\n\t\t\t\t□ Final Statement Balance: [Amount, if provided].\r\n\t\t\t\t□ Substantiated Amount: [Amount].\r\n\t\t\t\t□ Percentage Substantiated: [Percentage].\r\n\t\t○ Conclusion: \r\n\t\t\t§ If 51% or more is substantiated, the claim is valid.\r\n\t\t\t§ If less than 51% is substantiated, the claim is inadequately supported.\r\n\t7. Has the complaint been properly served to the defendant?\r\n\t\t○ Review the Sheriff’s Return of Service or equivalent document for details.\r\n\t\t○ Look for clear indications of: \r\n\t\t\t§ Successful service: \"Served the Complaint in Civil Action.\"\r\n\t\t\t§ Unsuccessful service: \"Not Found\" or other relevant notes.\r\n\t\t○ Assess any deputy notes about attempts to serve, such as vacant properties or alternative service arrangements.\r\nException: For Notice of Appeal cases, service by mail is acceptable, and a Sheriff’s Return may not be required.\r\n\r\nFormatting Your Analysis\r\nFor each criterion, use the following format to provide a detailed and structured response:\r\n<analysis>\r\n1. Personal credit card debt and individual defendant:  \r\n[Detailed reasoning and evidence]\r\n2. Letter of notice:  \r\n[Detailed reasoning and evidence]\r\n3. Claim amount:  \r\n[Detailed reasoning and evidence]\r\n4. Proof of credit card debt:  \r\n[Detailed reasoning and evidence]\r\n5. Statute of limitation:  \r\n[Detailed reasoning and evidence]\r\n6. Debt ownership and chain of collectors:  \r\n[Detailed reasoning and evidence]\r\n7. Demonstrative Exhibit and 51% proof check:  \r\n[Detailed reasoning and evidence, including calculations if necessary]\r\n8. Service of the complaint:  \r\n[Detailed reasoning and evidence]\r\n</analysis>\r\n\r\nDecision Format\r\nAfter completing your analysis, decide whether the case is eligible for the Credit Card Diversion Program or should remain in the Litigation System. Use the following format for your decision:\r\n<decision>\r\nDecision: [Credit Card Diversion Program]  \r\nExplanation: [Summarize your reasoning, highlighting key criteria that influenced your decision. Clearly address any missing or insufficient evidence.]\r\n</decision>\r\n\r\n**Do not source or access any data outside the files provided here in vector store!**\r\n"),
//                //ResponseItem.CreateUserMessageItem("")
//            };


//            var options = new ResponseCreationOptions
//            {
//                MaxOutputTokenCount = 100000,
//                TextOptions = new ResponseTextOptions
//                {
//                    TextFormat = ResponseTextFormat.CreateTextFormat()
//                },
//                StoredOutputEnabled = true
//            };

//            var vectorStoreIds = new[] { storeId };
//            options.Tools.Add(ResponseTool.CreateFileSearchTool(vectorStoreIds));



//            OpenAIResponseClient responseClient = azureClient.GetOpenAIResponseClient(_modelName);
//            var response = await responseClient.CreateResponseAsync(inputItems, options);

//            //var response = await responseClient.CreateResponseAsync(content: BinaryContent.Create(BinaryData.FromObjectAsJson(payload)));

//            if (response.GetRawResponse().Status != 200)
//            {
//                _logger.LogError("OpenAI API call failed with status: {Status}", response.GetRawResponse().Status);
//            }

//            var rawContent = response.GetRawResponse().Content.ToString();

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

//            await vectorStoreClient.DeleteVectorStoreAsync(storeId);
//            await DeleteUploadedFileAsync(uploadedFiles.Select(f => f.FileId).ToList());


//            return;

//        }


//        private object CreateRequestPayloadMulti(string modelOrDeployment, List<(string FileId, string DocName)> uploadedFiles, string userPrompt)
//        {
//            var fileItems = uploadedFiles.Select(doc => (object)new
//            {
//                type = "input_file",
//                file_id = doc.FileId
//            }).ToList();

//            // Add the user's actual prompt
//            fileItems.Add(new { type = "input_text", text = userPrompt });

//            return new
//            {
//                model = modelOrDeployment,

//                prompt = new
//                {
//                    id = _responseAPIPromptId
//                },

//                input = new object[]
//                {
//                    new
//                    {
//                        role = "user",
//                        content = fileItems.ToArray()
//                    }
//                },

//                text = new
//                {
//                    format = new { type = "text" }
//                },

//                reasoning = new
//                {
//                    effort = "low"
//                },

//                max_output_tokens = 3000
//            };
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

//                //using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(textContentFromDocId!));
//                using var memoryStream = new MemoryStream(fileContent ?? Array.Empty<byte>());
//                var endpoint = new Uri(_apiBaseUrl);
//                var apiKey = new AzureKeyCredential(_apiKey!);

//                AzureOpenAIClient azureClient = new(endpoint, apiKey);
//                var fileClient = azureClient.GetOpenAIFileClient();
//                var uploaded = await fileClient.UploadFileAsync(memoryStream, Path.GetFileNameWithoutExtension(docName) + ".pdf", "assistants");
//                if (uploaded.Value.Id != null)
//                {
//                    uploadedFiles.Add((uploaded.Value.Id, Path.GetFileName(docName)));
//                }
//                else
//                {
//                    throw new Exception("File upload failed.");
//                }
//            }

//            return uploadedFiles;

//        }

//        private async Task DeleteUploadedFileAsync(List<string> fileIds)
//        {
//            if (fileIds == null || fileIds.Count == 0)
//                return;
//            try
//            {
//                //var openAI = new OpenAI.OpenAIClient(_apiKey);
//                var endpoint = new Uri(_apiBaseUrl);
//                var apiKey = new AzureKeyCredential(_apiKey!);

//                AzureOpenAIClient azureClient = new(endpoint, apiKey);
//                var fileClient = azureClient.GetOpenAIFileClient();
//                foreach (var fileId in fileIds)
//                {
//                    try
//                    {
//                        var deletionResult = await fileClient.DeleteFileAsync(fileId);

//                        if (deletionResult?.Value?.Deleted != true)
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
