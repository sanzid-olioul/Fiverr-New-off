using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.EntityFrameworkCore;
// using Syncfusion.EJ2.Linq; // keep if you actually need it

namespace LancasterCreditCardDiversion.Services
{
    public class TimeHostedCheckEligibilityService : IHostedService, IDisposable
    {
        private readonly ILogger<TimeHostedCheckEligibilityService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer? _timer;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private int _isRunning = 0; // simple reentrancy guard

        public TimeHostedCheckEligibilityService(
            ILogger<TimeHostedCheckEligibilityService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(CheckQueuedDocs, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        // IMPORTANT: Keep the original name for the Timer callback, but make it NON-async.
        // We delegate to a safe async runner so exceptions never crash the process.
        private void CheckQueuedDocs(object? state)
        {
            // Fire-and-forget safely (no async void)
            _ = SafeRunAsync();
        }

        // Runs one "tick" safely: prevents overlap, retries on transient failures, and swallows final exceptions.
        private async Task SafeRunAsync()
        {
            // don’t overlap with the next timer tick
            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
                return;

            try
            {
                await TryDoWorkWithRetriesAsync();
            }
            catch (Exception ex)
            {
                // swallow so the host/app stays up; we’ll try again on the next tick
                _logger.LogError(ex, "Background tick failed after retries; will try again next interval.");
            }
            finally
            {
                Volatile.Write(ref _isRunning, 0);
            }
        }

        // Tiny retry with exponential backoff (3 attempts).
        private async Task TryDoWorkWithRetriesAsync()
        {
            const int maxAttempts = 3;
            var delay = TimeSpan.FromSeconds(3);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await DoWorkOnceAsync();
                    return; // success
                }
                catch (OperationCanceledException)
                {
                    // if you add cancellation in the future, bubble it
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "DB work attempt {Attempt} failed.", attempt);
                    if (attempt == maxAttempts)
                        throw;

                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                }
            }
        }

        // Your original logic moved here (unchanged semantics)
        private async Task DoWorkOnceAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                //var context = scope.ServiceProvider.GetRequiredService<PaLancCcdpDevDbContext>();

                PaLancCcdpDevDbContext context;
                try
                {
                    context = scope.ServiceProvider.GetRequiredService<PaLancCcdpDevDbContext>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DbContext could not be created. Database or connection string unavailable.");
                    return;
                }


                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                await context.ResponsesApiRequests
                    .Where(r => r.IsInProgress == true
                             && (r.EligibilityCheckStatus == "queued" || r.EligibilityCheckStatus == "in_progress")
                             && r.ModifiedDttm <= cutoff)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.IsInProgress, r => false)
                        .SetProperty(r => r.ModifiedDttm, r => DateTime.UtcNow)
                    );

                var queuedRequestIds = await context.ResponsesApiRequests
                    .Where(r => r.EligibilityCheckStatus == "queued" || r.EligibilityCheckStatus == "in_progress")
                    .Where(r => r.IsInProgress == false)
                    .Select(r => r.ReqId)
                    .ToListAsync();

                var documentsByReqId = new Dictionary<int, List<CaseDocumentViewModel>>();
                foreach (var reqId in queuedRequestIds)
                {
                    // Fetch document IDs for the current Req_Id
                    var reqDocIds = await context.EligibilityCheckRequestDocuments
                        .Where(rd => rd.ReqId == reqId)
                        .Select(rd => rd.DocId)
                        .ToListAsync();

                    // Fetch the actual documents using the document IDs
                    var queuedDocList = await context.CaseDocuments
                        .Where(cd => reqDocIds.Contains(cd.DocId))
                        .ToListAsync();

                    // Convert to ViewModel and store in the dictionary
                    var documents = queuedDocList.Select(cd => new CaseDocumentViewModel
                    {
                        DocId = cd.DocId,
                        Name = cd.Name,
                        DocDate = cd.DocDate,
                        Content = cd.Content,
                        DocType = cd.DocType,
                        DocTypeDomainName = cd.DocTypeDomainName,
                        CaseId = cd.CaseId,
                        TextContent = cd.TextContent,
                    }).ToList();

                    documentsByReqId[reqId] = documents;
                }

                var openAIService = scope.ServiceProvider.GetRequiredService<OpenAIService>();

                
                foreach (var reqId in queuedRequestIds)
                {
                    await _semaphore.WaitAsync(); // Wait until it is safe to enter
                    try
                    {
                        // Claim it (set IS_IN_PROGRESS = true)
                        var request = await context.ResponsesApiRequests
                            .FirstOrDefaultAsync(r => r.ReqId == reqId);

                        if (request == null || request.IsInProgress == true)
                        {
                            // Already claimed by someone else, skip
                            continue;
                        }

                        request.IsInProgress = true;
                        request.EligibilityCheckStatus = "in_progress";
                        request.ModifiedDttm = DateTime.UtcNow;
                        await context.SaveChangesAsync();

                        try
                        {
                            // Check if there are documents for the current Req_Id
                            if (documentsByReqId.TryGetValue(reqId, out var documents))
                            {
                                // Call CheckDocumentEligibility for the specific Req_Id with its documents
                                await openAIService.CheckDocumentEligibility(documents, reqId);
                            }
                        }
                        catch (Exception exItem)
                        {
                            // Per-item safety: log but still release claim
                            _logger.LogError(exItem, "Error processing ReqId {ReqId}. Releasing claim.", reqId);
                        }
                        finally
                        {
                            // Release claim (set IS_IN_PROGRESS = false)
                            request.IsInProgress = false;
                            request.ModifiedDttm = DateTime.UtcNow;
                            await context.SaveChangesAsync();
                        }
                    }
                    finally
                    {
                        _semaphore.Release(); // Ensure we release the semaphore even if an error occurs
                    }
                }
            }

            _logger.LogInformation("Timed Background Service is working.");
        }

        //Can be used to cancel tasks (Not implemented yet)
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
