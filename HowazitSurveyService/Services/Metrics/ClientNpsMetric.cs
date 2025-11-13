namespace HowazitSurveyService.Services.Metrics;

public sealed record ClientNpsMetric(string ClientId, double AverageNps, int ResponseCount);

