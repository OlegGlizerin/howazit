using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Repositories.Relational;

public interface IRelationalSurveyResponseRepository
{
    Task UpsertAsync(SurveyResponse response, string encryptedIpAddress, CancellationToken cancellationToken);
}

