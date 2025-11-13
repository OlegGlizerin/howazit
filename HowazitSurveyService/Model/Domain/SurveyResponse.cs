namespace HowazitSurveyService.Model.Domain;

public sealed class SurveyResponse
{
    public required string SurveyId { get; init; }

    public required string ClientId { get; init; }

    public required string ResponseId { get; init; }

    public required int NpsScore { get; init; }

    public required string Satisfaction { get; init; }

    public IDictionary<string, object?> CustomFields { get; init; } = new Dictionary<string, object?>();

    public required DateTimeOffset SubmittedAtUtc { get; init; }

    public string? UserAgent { get; init; }

    public string? IpAddress { get; init; }
}

