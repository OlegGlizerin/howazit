using System.Collections.Concurrent;

namespace HowazitSurveyService.Repositories.FastStorage;

public sealed class InMemoryFastSurveyResponseRepository : IFastSurveyResponseRepository
{
    private readonly ConcurrentDictionary<string, FastSurveyResponseRecord> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task UpsertAsync(FastSurveyResponseRecord record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = $"{record.ClientId}:{record.ResponseId}";
        _store.AddOrUpdate(key, record, (_, _) => record);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<FastSurveyResponseRecord>> GetByClientAsync(string clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = _store.Values.Where(v => string.Equals(v.ClientId, clientId, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IReadOnlyCollection<FastSurveyResponseRecord>>(results);
    }

    public Task<IReadOnlyCollection<FastSurveyResponseRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = _store.Values.ToList();
        return Task.FromResult<IReadOnlyCollection<FastSurveyResponseRecord>>(snapshot);
    }
}

