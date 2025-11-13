using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Metrics;

public interface IMetricsService
{
    void Track(SurveyResponse response);

    IReadOnlyCollection<ClientNpsMetric> GetClientNpsMetrics();
}

