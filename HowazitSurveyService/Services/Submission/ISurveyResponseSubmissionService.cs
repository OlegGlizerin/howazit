using HowazitSurveyService.Model.Dtos;

namespace HowazitSurveyService.Services.Submission;

public interface ISurveyResponseSubmissionService
{
    Task<SurveySubmissionResult> SubmitAsync(SurveyResponseRequest request, CancellationToken cancellationToken);
}

