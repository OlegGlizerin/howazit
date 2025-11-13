using FluentValidation;
using FluentValidation.Results;
using HowazitSurveyService.Model.Domain;
using HowazitSurveyService.Model.Dtos;
using HowazitSurveyService.Services.Messaging;
using HowazitSurveyService.Services.Sanitization;
using HowazitSurveyService.Services.Submission;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HowazitSurveyService.Tests.Services.Submission;

public sealed class SurveyResponseSubmissionServiceTests
{
    [Fact]
    public async Task SubmitAsync_QueuesResponse_ReturnsIdentifiers()
    {
        // Arrange
        var queue = Substitute.For<ISurveyResponseQueueService>();
        var sanitizer = Substitute.For<ISurveyResponseSanitizerService>();
        var validator = Substitute.For<IValidator<SurveyResponseRequest>>();
        var logger = Substitute.For<ILogger<SurveyResponseSubmissionService>>();

        var request = CreateRequest();
        sanitizer.Sanitize(request).Returns(request);
        validator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult());

        SurveyResponse? captured = null;
        queue.EnqueueAsync(Arg.Do<SurveyResponse>(sr => captured = sr), Arg.Any<CancellationToken>())
             .Returns(ValueTask.CompletedTask);

        var service = new SurveyResponseSubmissionService(queue, sanitizer, validator, logger);

        // Act
        var result = await service.SubmitAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(request.ResponseId, captured!.ResponseId);
        Assert.Equal(request.ClientId, captured.ClientId);
        Assert.Equal(request.ResponseId, result.ResponseId);
        Assert.Equal(request.ClientId, result.ClientId);
    }

    [Fact]
    public async Task SubmitAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var queue = Substitute.For<ISurveyResponseQueueService>();
        var sanitizer = Substitute.For<ISurveyResponseSanitizerService>();
        var validator = Substitute.For<IValidator<SurveyResponseRequest>>();
        var logger = Substitute.For<ILogger<SurveyResponseSubmissionService>>();

        var request = CreateRequest();
        sanitizer.Sanitize(request).Returns(request);
        var failures = new[]
        {
            new ValidationFailure("Responses.NpsScore", "Invalid")
        };
        validator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult(failures));

        var service = new SurveyResponseSubmissionService(queue, sanitizer, validator, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.SubmitAsync(request, CancellationToken.None));
        await queue.DidNotReceive().EnqueueAsync(Arg.Any<SurveyResponse>(), Arg.Any<CancellationToken>());
    }

    private static SurveyResponseRequest CreateRequest()
    {
        return new SurveyResponseRequest
        {
            SurveyId = "survey-1",
            ClientId = "client-1",
            ResponseId = "response-1",
            Responses = new SurveyResponsesBody
            {
                NpsScore = 9,
                Satisfaction = "satisfied",
                CustomFields = new Dictionary<string, object?>()
            },
            Metadata = new SurveyResponseMetadata
            {
                Timestamp = DateTimeOffset.UtcNow,
                UserAgent = "agent",
                IpAddress = "127.0.0.1"
            }
        };
    }
}

