namespace HowazitSurveyService.Services.Insights;

public interface IInsightsCacheService
{
    IReadOnlyCollection<ClientInsights> GetLatest();

    void SetInsights(IEnumerable<ClientInsights> insights);
}

