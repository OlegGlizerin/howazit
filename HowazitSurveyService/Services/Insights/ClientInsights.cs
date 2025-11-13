namespace HowazitSurveyService.Services.Insights;

public sealed record ClientInsights(string ClientId, IReadOnlyDictionary<string, int> SatisfactionCounts, DateTimeOffset GeneratedAt);

