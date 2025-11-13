using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Processing;

public interface ISurveyResponseProcessorService
{
    Task ProcessAsync(SurveyResponse response, CancellationToken cancellationToken);
}

