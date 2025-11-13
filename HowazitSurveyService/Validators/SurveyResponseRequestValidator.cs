using FluentValidation;
using HowazitSurveyService.Model.Dtos;

namespace HowazitSurveyService.Validators;

public sealed class SurveyResponseRequestValidator : AbstractValidator<SurveyResponseRequest>
{
    public SurveyResponseRequestValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ResponseId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Responses)
            .NotNull()
            .SetValidator(new SurveyResponsesBodyValidator());

        RuleFor(x => x.Metadata)
            .NotNull()
            .SetValidator(new SurveyResponseMetadataValidator());
    }
}

internal sealed class SurveyResponsesBodyValidator : AbstractValidator<SurveyResponsesBody>
{
    public SurveyResponsesBodyValidator()
    {
        RuleFor(x => x.NpsScore)
            .NotNull()
            .InclusiveBetween(0, 10);

        RuleFor(x => x.Satisfaction)
            .NotEmpty()
            .MaximumLength(50);
    }
}

internal sealed class SurveyResponseMetadataValidator : AbstractValidator<SurveyResponseMetadata>
{
    public SurveyResponseMetadataValidator()
    {
        RuleFor(x => x.Timestamp)
            .NotNull()
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow.AddMinutes(5));

        RuleFor(x => x.UserAgent)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .Matches(@"^(?:\d{1,3}\.){3}\d{1,3}$")
            .WithMessage("metadata.ip_address must be a valid IPv4 address");
    }
}

