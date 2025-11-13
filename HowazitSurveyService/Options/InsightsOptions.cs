namespace HowazitSurveyService.Options;

public sealed class InsightsOptions
{
    public const string SectionName = "Insights";

    public int RefreshIntervalSeconds { get; init; } = 60;
}

