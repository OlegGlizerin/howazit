using HowazitSurveyService.Model.Domain;
using HowazitSurveyService.Repositories.FastStorage;
using HowazitSurveyService.Repositories.Relational;
using HowazitSurveyService.Services.Metrics;
using HowazitSurveyService.Services.Security;
using Microsoft.Extensions.Logging;

namespace HowazitSurveyService.Services.Processing;

public sealed class SurveyResponseProcessorService : ISurveyResponseProcessorService
{
    private readonly IRelationalSurveyResponseRepository _relationalRepository;
    private readonly IFastSurveyResponseRepository _fastRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<SurveyResponseProcessorService> _logger;

    public SurveyResponseProcessorService(
        IRelationalSurveyResponseRepository relationalRepository,
        IFastSurveyResponseRepository fastRepository,
        IEncryptionService encryptionService,
        IMetricsService metricsService,
        ILogger<SurveyResponseProcessorService> logger)
    {
        _relationalRepository = relationalRepository;
        _fastRepository = fastRepository;
        _encryptionService = encryptionService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task ProcessAsync(SurveyResponse response, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing survey response {ResponseId} for client {ClientId}", response.ResponseId, response.ClientId);

        var encryptedIp = response.IpAddress is null ? string.Empty : _encryptionService.Encrypt(response.IpAddress);

        await _relationalRepository.UpsertAsync(response, encryptedIp, cancellationToken).ConfigureAwait(false);

        var record = new FastSurveyResponseRecord(
            response.ClientId,
            response.ResponseId,
            response.NpsScore,
            response.Satisfaction,
            response.SubmittedAtUtc,
            response.CustomFields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            response.UserAgent);

        await _fastRepository.UpsertAsync(record, cancellationToken).ConfigureAwait(false);

        _metricsService.Track(response);
    }
}


