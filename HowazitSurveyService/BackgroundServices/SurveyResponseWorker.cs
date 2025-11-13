using HowazitSurveyService.Options;
using HowazitSurveyService.Services.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using HowazitSurveyService.Services.Processing;

namespace HowazitSurveyService.BackgroundServices;

public sealed class SurveyResponseWorker : BackgroundService
{
    private readonly ISurveyResponseQueueService _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SurveyResponseWorker> _logger;
    private readonly SurveyProcessingOptions _options;

    public SurveyResponseWorker(
        ISurveyResponseQueueService queue,
        IServiceScopeFactory scopeFactory,
        IOptions<SurveyProcessingOptions> options,
        ILogger<SurveyResponseWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value ?? new SurveyProcessingOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await ProcessAsync(item, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process survey response {ResponseId} attempt {Attempt}", item.Response.ResponseId, item.Attempt);

                if (item.Attempt + 1 >= _options.MaxRetryAttempts)
                {
                    _logger.LogCritical("Dropping survey response {ResponseId} after {Attempts} attempts", item.Response.ResponseId, _options.MaxRetryAttempts);
                    continue;
                }

                if (_options.RetryDelayMilliseconds > 0)
                {
                    await Task.Delay(_options.RetryDelayMilliseconds, stoppingToken).ConfigureAwait(false);
                }

                await _queue.RequeueAsync(item, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessAsync(SurveyResponseQueueItem item, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ISurveyResponseProcessorService>();
        await processor.ProcessAsync(item.Response, cancellationToken).ConfigureAwait(false);
    }
}

