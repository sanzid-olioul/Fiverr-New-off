using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LancasterCreditCardDiversion.Data;
using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.Services;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using SmartComponents.Inference.OpenAI;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);

// Setup key vault 
var kvUri = builder.Configuration.GetValue<string>("AzureKeyVault") ?? throw new InvalidOperationException("Configuration value 'AzureKeyValult' is required.");

DefaultAzureCredentialOptions defaultAzureCredentialOptions = new()
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeAzureCliCredential = false,
    ExcludeAzurePowerShellCredential = true,
    ExcludeAzureDeveloperCliCredential = true,
    ExcludeInteractiveBrowserCredential = true
};

var kvClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(defaultAzureCredentialOptions));
builder.Configuration.AddAzureKeyVault(kvClient, new AzureKeyVaultConfigurationOptions());

// Register Syncfusion license-31
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey-31"]);

// Configure SMTP options and SmartComponents settings
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

var isAzure = builder.Configuration.GetValue<bool>("UseAzure");

var smartComponentsSection = builder.Configuration.GetSection("SmartComponents");
var azureKey = builder.Configuration["AzureOpenAI:ApiKey"];
var openAiKey = builder.Configuration["OpenAI:ApiKey-DotNet"];

var apiKey =
    isAzure
        ? azureKey ?? throw new InvalidOperationException(
            "OpenAI:UseAzure=true but AzureOpenAI:ApiKey is missing.")
        : openAiKey ?? throw new InvalidOperationException(
            "OpenAI:UseAzure=false but OpenAI:ApiKey-DotNet is missing.");


builder.Services.Configure<OpenAIConfigViewModel>(opts =>
{
    opts.ApiKey = apiKey;
    opts.ApiBaseUrl = builder.Configuration["OpenAI:ApiBaseUrl"]!;
    opts.ModelName = builder.Configuration["OpenAI:ModelName"]!;
});


// Add caching, session, and CORS services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(120);
});

// Register services for dependency injection
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();

// Add JSON options to handle reference cycles
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure logging with NLog
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Host.UseNLog();

//// Configure database context with Oracle provider
//builder.Services.AddDbContext<PaLancCcdpDevDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Replace the Oracle DbContext registration with SQL Server
builder.Services.AddDbContext<PaLancCcdpDevDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LancasterCreditCardDiversion-palanc-Connection")));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LancasterCreditCardDiversion-palanc-Connection")));

builder.Services.Configure<HostOptions>(o =>
{
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddHttpClient("OpenAI_LongRunning", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

// Add HTTP client and hosted services
builder.Services.AddHostedService<TimeHostedCheckEligibilityService>();

// Add scoped and transient services
builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<CaseService>();
builder.Services.AddScoped<LetterTemplatesService>();
builder.Services.AddScoped<CaseCommentsService>();
builder.Services.AddScoped<CaseDocumentsService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HearingDatesService>();
builder.Services.AddScoped<EligibilityCheckRequestsService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<CaseStatusClass>();
builder.Services.AddScoped<SessionAndMergeFieldManagerService>();

// Configure SmartComponents with OpenAI inference backend
builder.Services.AddSmartComponents()
    .WithInferenceBackend<OpenAIInferenceBackend>();


var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// appinfo
app.MapGet("/appInfo", () => new
{
    Name = app.Configuration["Application:Name"] ?? app.Environment.ApplicationName,
    Version = app.Configuration["Application:Version"],
    Environment = app.Environment.EnvironmentName,
    ReleaseName = app.Configuration["Application:ReleaseName"],
    BuildNumber = app.Configuration["Application:BuildNumber"],
}).RequireCors(policy => policy.AllowAnyOrigin());

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cases}/{action=Index}/{id?}");

app.Run();


//using LancasterCreditCardDiversion.Data;
//using LancasterCreditCardDiversion.Models;
//using LancasterCreditCardDiversion.Services;
//using LancasterCreditCardDiversion.ViewModels;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.EntityFrameworkCore;
//using NLog.Web;
//using SmartComponents.Inference.OpenAI;

//var builder = WebApplication.CreateBuilder(args);

//// Register Syncfusion license key
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration.GetValue<string>("Syncfusion:LicenseKey"));

//// Configure SMTP options and SmartComponents settings
//builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
//builder.Services.Configure<OpenAIConfigViewModel>(builder.Configuration.GetSection("OpenAI"));
//builder.Services.Configure<SmartComponentsViewModel>(builder.Configuration.GetSection("SmartComponents"));

//// Add caching, session, and CORS services
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddCors();
//builder.Services.AddControllersWithViews();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddSession(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//    options.IdleTimeout = TimeSpan.FromMinutes(120);
//});

//// Register services for dependency injection
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();

//// Add JSON options to handle reference cycles
//builder.Services.AddControllersWithViews()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//    });

//// Configure logging with NLog
//builder.Logging.SetMinimumLevel(LogLevel.Information);
//builder.Host.UseNLog();

//// Replace the Oracle DbContext registration with SQL Server
//builder.Services.AddDbContext<PaLancCcdpDevDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<AuthDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.Configure<HostOptions>(o =>
//{
//    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
//});


//// Add HTTP client and hosted services
//builder.Services.AddHttpClient();
//builder.Services.AddHostedService<TimeHostedCheckEligibilityService>();

//// Add scoped and transient services
//builder.Services.AddScoped<CommonService>();
//builder.Services.AddScoped<CaseService>();
//builder.Services.AddScoped<LetterTemplatesService>();
//builder.Services.AddScoped<CaseCommentsService>();
//builder.Services.AddScoped<CaseDocumentsService>();
//builder.Services.AddScoped<AuthService>();
//builder.Services.AddScoped<UserService>();
//builder.Services.AddScoped<HearingDatesService>();
//builder.Services.AddScoped<EligibilityCheckRequestsService>();
//builder.Services.AddTransient<EmailService>();
//builder.Services.AddScoped<OpenAIService>();
//builder.Services.AddScoped<CaseStatusClass>();
//builder.Services.AddScoped<SessionAndMergeFieldManagerService>();


//// Configure SmartComponents with OpenAI inference backend
//builder.Services.AddSmartComponents()
//    .WithInferenceBackend<OpenAIInferenceBackend>();

//var app = builder.Build();

//// Configure middleware pipeline
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseSession();
//app.UseAuthentication();
//app.UseAuthorization();

//// Configure default route
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Cases}/{action=Index}/{id?}");

//app.Run();
