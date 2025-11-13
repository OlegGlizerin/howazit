using HowazitSurveyService.Options;
using HowazitSurveyService.Repositories.FastStorage;
using HowazitSurveyService.Services.Insights;
using Microsoft.Extensions.Options;

namespace HowazitSurveyService.BackgroundServices;

public sealed class InsightsBackgroundService : BackgroundService
{
    private readonly IFastSurveyResponseRepository _fastRepository;
    private readonly IInsightsCacheService _cache;
    private readonly ILogger<InsightsBackgroundService> _logger;
    private readonly InsightsOptions _options;

    public InsightsBackgroundService(
        IFastSurveyResponseRepository fastRepository,
        IInsightsCacheService cache,
        IOptions<InsightsOptions> options,
        ILogger<InsightsBackgroundService> logger)
    {
        _fastRepository = fastRepository;
        _cache = cache;
        _logger = logger;
        _options = options.Value ?? new InsightsOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Insights background service started with interval {Interval} seconds", _options.RefreshIntervalSeconds);

        // Run first refresh immediately, then continue with intervals
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Triggering insights refresh cycle...");
                await RefreshInsightsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Insights background service is shutting down...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh insights. Will retry after delay.");
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var delay = TimeSpan.FromSeconds(Math.Max(5, _options.RefreshIntervalSeconds));
            _logger.LogDebug("Waiting {DelaySeconds} seconds before next insights refresh...", delay.TotalSeconds);
            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
        }
        
        _logger.LogInformation("Insights background service stopped.");
    }

    private async Task RefreshInsightsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting insights refresh...");
        
        var records = await _fastRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Retrieved {Count} records from fast repository", records.Count);
        
        var grouped = records
            .GroupBy(r => r.ClientId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var counts = group
                    .GroupBy(r => r.Satisfaction ?? "unknown", StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                return new ClientInsights(group.Key, counts, DateTimeOffset.UtcNow);
            })
            .ToList();

        _cache.SetInsights(grouped);
        _logger.LogInformation("Insights refreshed successfully. Generated insights for {ClientCount} clients", grouped.Count);
    }
}

