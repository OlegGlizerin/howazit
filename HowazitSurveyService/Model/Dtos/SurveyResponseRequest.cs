using System.Text.Json.Serialization;

namespace HowazitSurveyService.Model.Dtos;

public sealed record SurveyResponseRequest
{
    [JsonPropertyName("surveyId")]
    public required string SurveyId { get; init; }

    [JsonPropertyName("clientId")]
    public required string ClientId { get; init; }

    [JsonPropertyName("responseId")]
    public required string ResponseId { get; init; }

    [JsonPropertyName("responses")]
    public required SurveyResponsesBody Responses { get; init; }

    [JsonPropertyName("metadata")]
    public required SurveyResponseMetadata Metadata { get; init; }
}

public sealed record SurveyResponsesBody
{
    [JsonPropertyName("nps_score")]
    public int? NpsScore { get; init; }

    [JsonPropertyName("satisfaction")]
    public string? Satisfaction { get; init; }

    [JsonPropertyName("custom_fields")]
    public IDictionary<string, object?>? CustomFields { get; init; }
}

public sealed record SurveyResponseMetadata
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; init; }

    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; init; }

    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; init; }
}

