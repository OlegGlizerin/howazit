namespace HowazitSurveyService.Options;

public sealed class SurveyProcessingOptions
{
    public const string SectionName = "SurveyProcessing";

    public int MaxRetryAttempts { get; init; } = 5;

    public int RetryDelayMilliseconds { get; init; } = 2000;
}

