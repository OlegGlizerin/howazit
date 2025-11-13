using System.Text.Json;
using HowazitSurveyService.BackgroundServices;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using HowazitSurveyService.Model;
using HowazitSurveyService.Options;
using HowazitSurveyService.Repositories.FastStorage;
using HowazitSurveyService.Repositories.Relational;
using HowazitSurveyService.Services.Insights;
using HowazitSurveyService.Services.Messaging;
using HowazitSurveyService.Services.Metrics;
using HowazitSurveyService.Services.Processing;
using HowazitSurveyService.Services.Sanitization;
using HowazitSurveyService.Services.Security;
using HowazitSurveyService.Services.Submission;
using HowazitSurveyService.Validators;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddValidatorsFromAssemblyContaining<SurveyResponseRequestValidator>();

builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection(EncryptionOptions.SectionName));
builder.Services.Configure<SurveyProcessingOptions>(builder.Configuration.GetSection(SurveyProcessingOptions.SectionName));
builder.Services.Configure<InsightsOptions>(builder.Configuration.GetSection(InsightsOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Sqlite") 
    ?? "Data Source=HowazitSurveyService.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISurveyResponseQueueService, InMemorySurveyResponseQueueService>();
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<ISurveyResponseSanitizerService, SurveyResponseSanitizerService>();
builder.Services.AddSingleton<IFastSurveyResponseRepository, InMemoryFastSurveyResponseRepository>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<IInsightsCacheService, InsightsCacheService>();

builder.Services.AddScoped<IRelationalSurveyResponseRepository, SqlSurveyResponseRepository>();
builder.Services.AddScoped<ISurveyResponseProcessorService, SurveyResponseProcessorService>();
builder.Services.AddScoped<ISurveyResponseSubmissionService, SurveyResponseSubmissionService>();

builder.Services.AddHostedService<SurveyResponseWorker>();
builder.Services.AddHostedService<InsightsBackgroundService>();
builder.Services.AddHostedService<HealthCheckBackgroundService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("HowazitSurveyService"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation()
               .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
               .AddConsoleExporter();
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirection is disabled to support plain HTTP development endpoints.

var healthCheckOptions = new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checkedAtUtc = DateTimeOffset.UtcNow,
            entries = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
};

app.MapHealthChecks("/health", healthCheckOptions);

app.MapControllers();

await EnsureDatabaseCreatedAsync(app.Services);
await WarmUpFastStorageAsync(app.Services);

app.Run();

static async Task EnsureDatabaseCreatedAsync(IServiceProvider services)
{
    try
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Creating SQLite database if it doesn't exist...");
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database ensured successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.CreateScope().ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to create database: {Error}", ex.Message);
        throw;
    }
}

static async Task WarmUpFastStorageAsync(IServiceProvider services)
{
    try
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var fastRepository = scope.ServiceProvider.GetRequiredService<IFastSurveyResponseRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Warming up fast storage from database...");
        
        var entities = await dbContext.SurveyResponses.ToListAsync();
        logger.LogInformation("Found {Count} records in database to load into fast storage", entities.Count);
        
        var loadedCount = 0;
        foreach (var entity in entities)
        {
            var customFields = DeserializeCustomFields(entity.CustomFieldsJson);
            
            var record = new FastSurveyResponseRecord(
                entity.ClientId,
                entity.ResponseId,
                entity.NpsScore,
                entity.Satisfaction,
                entity.SubmittedAtUtc,
                customFields,
                entity.UserAgent);
            
            await fastRepository.UpsertAsync(record, CancellationToken.None);
            loadedCount++;
        }
        
        logger.LogInformation("Fast storage warm-up completed. Loaded {Count} records", loadedCount);
    }
    catch (Exception ex)
    {
        var logger = services.CreateScope().ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to warm up fast storage: {Error}", ex.Message);
        // Don't throw - allow app to continue even if warm-up fails
    }
}

static IReadOnlyDictionary<string, object?> DeserializeCustomFields(string? customFieldsJson)
{
    if (string.IsNullOrWhiteSpace(customFieldsJson))
    {
        return new Dictionary<string, object?>();
    }
    
    try
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(customFieldsJson);
        return dict ?? new Dictionary<string, object?>();
    }
    catch
    {
        return new Dictionary<string, object?>();
    }
}
