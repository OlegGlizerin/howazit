namespace HowazitSurveyService.Model.Entities;

public sealed class SurveyResponseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string SurveyId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ResponseId { get; set; } = string.Empty;

    public int NpsScore { get; set; }

    public string Satisfaction { get; set; } = string.Empty;

    public string? CustomFieldsJson { get; set; }

    public DateTimeOffset SubmittedAtUtc { get; set; }

    public string? UserAgent { get; set; }

    public string? EncryptedIpAddress { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

