using System.Collections.Concurrent;

namespace HowazitSurveyService.Services.Insights;

public sealed class InsightsCacheService : IInsightsCacheService
{
    private IReadOnlyCollection<ClientInsights> _snapshot = Array.Empty<ClientInsights>();
    private readonly object _lock = new();

    public IReadOnlyCollection<ClientInsights> GetLatest()
    {
        lock (_lock)
        {
            return _snapshot;
        }
    }

    public void SetInsights(IEnumerable<ClientInsights> insights)
    {
        lock (_lock)
        {
            _snapshot = insights.ToList();
        }
    }
}


