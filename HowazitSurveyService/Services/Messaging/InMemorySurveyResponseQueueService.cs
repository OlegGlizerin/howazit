using System.Threading.Channels;
using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Messaging;

public sealed class InMemorySurveyResponseQueueService : ISurveyResponseQueueService
{
    private readonly Channel<SurveyResponseQueueItem> _channel;

    public InMemorySurveyResponseQueueService(int capacity = 10_000)
    {
        _channel = Channel.CreateBounded<SurveyResponseQueueItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(SurveyResponse response, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(new SurveyResponseQueueItem(response, Attempt: 0), cancellationToken);
    }

    public async IAsyncEnumerable<SurveyResponseQueueItem> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }

    public ValueTask RequeueAsync(SurveyResponseQueueItem item, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(item with { Attempt = item.Attempt + 1 }, cancellationToken);
    }
}


