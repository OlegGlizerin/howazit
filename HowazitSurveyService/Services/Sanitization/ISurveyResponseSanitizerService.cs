using HowazitSurveyService.Model.Dtos;

namespace HowazitSurveyService.Services.Sanitization;

public interface ISurveyResponseSanitizerService
{
    SurveyResponseRequest Sanitize(SurveyResponseRequest request);
}

