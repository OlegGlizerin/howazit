using HowazitSurveyService.Model.Domain;

namespace HowazitSurveyService.Services.Messaging;

public sealed record SurveyResponseQueueItem(SurveyResponse Response, int Attempt);

