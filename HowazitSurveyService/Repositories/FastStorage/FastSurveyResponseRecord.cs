namespace HowazitSurveyService.Repositories.FastStorage;

public sealed record FastSurveyResponseRecord(
    string ClientId,
    string ResponseId,
    int NpsScore,
    string Satisfaction,
    DateTimeOffset SubmittedAtUtc,
    IReadOnlyDictionary<string, object?> CustomFields,
    string? UserAgent);

