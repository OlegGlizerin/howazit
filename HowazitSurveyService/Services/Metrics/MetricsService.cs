using System.Collections.Concurrent;
using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Metrics;

public sealed class MetricsService : IMetricsService
{
    private readonly ConcurrentDictionary<string, NpsAccumulator> _accumulators = new(StringComparer.OrdinalIgnoreCase);

    public void Track(SurveyResponse response)
    {
        var accumulator = _accumulators.GetOrAdd(response.ClientId, _ => new NpsAccumulator());
        accumulator.Add(response.NpsScore);
    }

    public IReadOnlyCollection<ClientNpsMetric> GetClientNpsMetrics()
    {
        return _accumulators.Select(pair =>
            new ClientNpsMetric(pair.Key, pair.Value.Average, pair.Value.Count)).ToList();
    }

    private sealed class NpsAccumulator
    {
        private long _count;
        private long _total;

        public void Add(int score)
        {
            Interlocked.Increment(ref _count);
            Interlocked.Add(ref _total, score);
        }

        public int Count => (int)Volatile.Read(ref _count);

        public double Average
        {
            get
            {
                var count = Count;
                if (count == 0)
                {
                    return 0;
                }

                var total = Volatile.Read(ref _total);
                return (double)total / count;
            }
        }
    }
}

