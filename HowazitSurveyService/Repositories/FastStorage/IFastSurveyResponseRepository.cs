namespace HowazitSurveyService.Repositories.FastStorage;

public interface IFastSurveyResponseRepository
{
    Task UpsertAsync(FastSurveyResponseRecord record, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<FastSurveyResponseRecord>> GetByClientAsync(string clientId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<FastSurveyResponseRecord>> GetAllAsync(CancellationToken cancellationToken);
}

