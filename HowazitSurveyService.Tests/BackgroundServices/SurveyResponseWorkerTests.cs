using HowazitSurveyService.BackgroundServices;
using HowazitSurveyService.Model.Domain;
using HowazitSurveyService.Options;
using HowazitSurveyService.Services.Messaging;
using HowazitSurveyService.Services.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HowazitSurveyService.Tests.BackgroundServices;

public sealed class SurveyResponseWorkerTests
{
    [Fact]
    public async Task Worker_RetriesUntilMaxAttempts()
    {
        // Arrange
        var queue = new InMemorySurveyResponseQueueService();
        var processor = Substitute.For<ISurveyResponseProcessorService>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(ISurveyResponseProcessorService)).Returns(processor);
        scope.ServiceProvider.Returns(services);
        scopeFactory.CreateScope().Returns(scope);
        var options = Microsoft.Extensions.Options.Options.Create(new SurveyProcessingOptions
        {
            MaxRetryAttempts = 2,
            RetryDelayMilliseconds = 10
        });
        var logger = Substitute.For<ILogger<SurveyResponseWorker>>();
        var worker = new SurveyResponseWorker(queue, scopeFactory, options, logger);

        var response = new SurveyResponse
        {
            SurveyId = "s-1",
            ClientId = "c-1",
            ResponseId = "r-1",
            NpsScore = 9,
            Satisfaction = "happy",
            SubmittedAtUtc = DateTimeOffset.UtcNow,
            CustomFields = new Dictionary<string, object?>(),
            UserAgent = "agent",
            IpAddress = "127.0.0.1"
        };

        processor.ProcessAsync(response, Arg.Any<CancellationToken>())
                 .ThrowsAsync(new InvalidOperationException("boom"));

        await queue.EnqueueAsync(response, CancellationToken.None);

        // Act
        await worker.StartAsync(CancellationToken.None);

        await Task.Delay(120);

        await worker.StopAsync(CancellationToken.None);

        // Assert
        await processor.Received(2).ProcessAsync(response, Arg.Any<CancellationToken>());
    }
}

