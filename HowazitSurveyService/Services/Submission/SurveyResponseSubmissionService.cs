using FluentValidation;
using FluentValidation.Results;
using HowazitSurveyService.Model.Domain;
using HowazitSurveyService.Model.Dtos;
using HowazitSurveyService.Services.Messaging;
using HowazitSurveyService.Services.Sanitization;
using Microsoft.Extensions.Logging;

namespace HowazitSurveyService.Services.Submission;

public sealed class SurveyResponseSubmissionService : ISurveyResponseSubmissionService
{
    private readonly ISurveyResponseQueueService _queue;
    private readonly ISurveyResponseSanitizerService _sanitizer;
    private readonly IValidator<SurveyResponseRequest> _validator;
    private readonly ILogger<SurveyResponseSubmissionService> _logger;

    public SurveyResponseSubmissionService(
        ISurveyResponseQueueService queue,
        ISurveyResponseSanitizerService sanitizer,
        IValidator<SurveyResponseRequest> validator,
        ILogger<SurveyResponseSubmissionService> logger)
    {
        _queue = queue;
        _sanitizer = sanitizer;
        _validator = validator;
        _logger = logger;
    }

    public async Task<SurveySubmissionResult> SubmitAsync(SurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var sanitized = _sanitizer.Sanitize(request);
        ValidationResult validation = await _validator.ValidateAsync(sanitized, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var domain = MapToDomain(sanitized);

        await _queue.EnqueueAsync(domain, cancellationToken);

        _logger.LogInformation("Queued survey response {ResponseId} for client {ClientId}", domain.ResponseId, domain.ClientId);

        return new SurveySubmissionResult(domain.ResponseId, domain.ClientId);
    }

    private static SurveyResponse MapToDomain(SurveyResponseRequest request)
    {
        var submittedAt = request.Metadata.Timestamp ?? DateTimeOffset.UtcNow;

        return new SurveyResponse
        {
            SurveyId = request.SurveyId,
            ClientId = request.ClientId,
            ResponseId = request.ResponseId,
            NpsScore = request.Responses.NpsScore!.Value,
            Satisfaction = request.Responses.Satisfaction!,
            CustomFields = request.Responses.CustomFields ?? new Dictionary<string, object?>(),
            SubmittedAtUtc = submittedAt.ToUniversalTime(),
            UserAgent = request.Metadata.UserAgent,
            IpAddress = request.Metadata.IpAddress
        };
    }
}

public sealed record SurveySubmissionResult(string ResponseId, string ClientId);

