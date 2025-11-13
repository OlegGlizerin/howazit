using System.Linq;
using HowazitSurveyService.Model.Domain;
using HowazitSurveyService.Repositories.FastStorage;
using HowazitSurveyService.Repositories.Relational;
using HowazitSurveyService.Services.Metrics;
using HowazitSurveyService.Services.Processing;
using HowazitSurveyService.Services.Security;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HowazitSurveyService.Tests.Services.Processing;

public sealed class SurveyResponseProcessorTests
{
    [Fact]
    public async Task ProcessAsync_EncryptsIpAndPersistsToRepositories()
    {
        // Arrange
        var relational = Substitute.For<IRelationalSurveyResponseRepository>();
        var fast = Substitute.For<IFastSurveyResponseRepository>();
        var encryption = Substitute.For<IEncryptionService>();
        var metrics = Substitute.For<IMetricsService>();
        var logger = Substitute.For<ILogger<SurveyResponseProcessorService>>();

        var processor = new SurveyResponseProcessorService(relational, fast, encryption, metrics, logger);

        var response = new SurveyResponse
        {
            SurveyId = "survey-1",
            ClientId = "client-1",
            ResponseId = "response-1",
            NpsScore = 8,
            Satisfaction = "satisfied",
            CustomFields = new Dictionary<string, object?>
            {
                ["foo"] = "bar"
            },
            SubmittedAtUtc = DateTimeOffset.UtcNow,
            UserAgent = "unit-test",
            IpAddress = "127.0.0.1"
        };

        encryption.Encrypt("127.0.0.1").Returns("encrypted-ip");

        // Act
        await processor.ProcessAsync(response, CancellationToken.None);

        // Assert
        await relational.Received(1).UpsertAsync(response, "encrypted-ip", Arg.Any<CancellationToken>());

        var call = fast.ReceivedCalls().Single();
        var arguments = call.GetArguments();
        var capturedRecord = Assert.IsType<FastSurveyResponseRecord>(arguments[0]);

        Assert.Equal(response.ClientId, capturedRecord.ClientId);
        Assert.Equal(response.ResponseId, capturedRecord.ResponseId);
        Assert.Equal(response.NpsScore, capturedRecord.NpsScore);
        Assert.Equal(response.Satisfaction, capturedRecord.Satisfaction);
        Assert.True(capturedRecord.CustomFields.TryGetValue("foo", out var value));
        Assert.Equal("bar", value?.ToString());

        metrics.Received(1).Track(response);
    }
}

