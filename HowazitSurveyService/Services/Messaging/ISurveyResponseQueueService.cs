using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Messaging;

public interface ISurveyResponseQueueService
{
    ValueTask EnqueueAsync(SurveyResponse response, CancellationToken cancellationToken);

    IAsyncEnumerable<SurveyResponseQueueItem> ReadAllAsync(CancellationToken cancellationToken);

    ValueTask RequeueAsync(SurveyResponseQueueItem item, CancellationToken cancellationToken);
}

