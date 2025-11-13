namespace HowazitSurveyService.Options;

public sealed class EncryptionOptions
{
    public const string SectionName = "Encryption";

    public required string Key { get; init; }

    public required string Iv { get; init; }
}

