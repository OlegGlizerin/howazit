using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HowazitSurveyService.BackgroundServices;

public sealed class HealthCheckBackgroundService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public HealthCheckBackgroundService(
        HealthCheckService healthCheckService,
        ILogger<HealthCheckBackgroundService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health check background service started. Checking every {Interval} minutes", _checkInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthCheckAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Health check background service is shutting down...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing health check. Will retry after delay.");
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Health check background service stopped.");
    }

    private async Task PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken).ConfigureAwait(false);

        var overallStatus = healthReport.Status;
        var timestamp = DateTimeOffset.UtcNow;

        if (overallStatus == HealthStatus.Healthy)
        {
            _logger.LogInformation(
                "[HEALTH CHECK] Service is UP and healthy at {Timestamp}. All checks passed: {CheckCount}",
                timestamp,
                healthReport.Entries.Count);
        }
        else if (overallStatus == HealthStatus.Degraded)
        {
            _logger.LogWarning(
                "[HEALTH CHECK] Service is UP but DEGRADED at {Timestamp}. Some checks failed: {CheckCount}",
                timestamp,
                healthReport.Entries.Count);
        }
        else
        {
            _logger.LogError(
                "[HEALTH CHECK] Service is UNHEALTHY at {Timestamp}. Failed checks: {CheckCount}",
                timestamp,
                healthReport.Entries.Count);
        }

        // Log individual check results
        foreach (var entry in healthReport.Entries)
        {
            var status = entry.Value.Status;
            var description = entry.Value.Description ?? "No description";
            var duration = entry.Value.Duration.TotalMilliseconds;

            if (status == HealthStatus.Healthy)
            {
                _logger.LogInformation(
                    "[HEALTH CHECK] ✓ {CheckName}: {Status} ({Duration}ms) - {Description}",
                    entry.Key,
                    status,
                    duration.ToString("F2"),
                    description);
            }
            else if (status == HealthStatus.Degraded)
            {
                _logger.LogWarning(
                    "[HEALTH CHECK] ⚠ {CheckName}: {Status} ({Duration}ms) - {Description}",
                    entry.Key,
                    status,
                    duration.ToString("F2"),
                    description);
            }
            else
            {
                _logger.LogError(
                    "[HEALTH CHECK] ✗ {CheckName}: {Status} ({Duration}ms) - {Description}",
                    entry.Key,
                    status,
                    duration.ToString("F2"),
                    description);

                // Log exception if available
                if (entry.Value.Exception != null)
                {
                    _logger.LogError(entry.Value.Exception, "[HEALTH CHECK] Exception for {CheckName}", entry.Key);
                }
            }
        }
    }
}

