using System.Text.RegularExpressions;
using HowazitSurveyService.Model.Dtos;

namespace HowazitSurveyService.Services.Sanitization;

public sealed partial class SurveyResponseSanitizerService : ISurveyResponseSanitizerService
{
    public SurveyResponseRequest Sanitize(SurveyResponseRequest request)
    {
        return request with
        {
            SurveyId = request.SurveyId.Trim(),
            ClientId = request.ClientId.Trim(),
            ResponseId = request.ResponseId.Trim(),
            Responses = request.Responses with
            {
                Satisfaction = request.Responses.Satisfaction?.Trim(),
                CustomFields = SanitizeCustomFields(request.Responses.CustomFields)
            },
            Metadata = request.Metadata with
            {
                UserAgent = request.Metadata.UserAgent?.Trim(),
                IpAddress = request.Metadata.IpAddress is null ? null : SanitizeIp(request.Metadata.IpAddress)
            }
        };
    }

    private static IDictionary<string, object?> SanitizeCustomFields(IDictionary<string, object?>? customFields)
    {
        if (customFields is null)
        {
            return new Dictionary<string, object?>();
        }

        return customFields.ToDictionary(
            pair => pair.Key.Trim(),
            pair => pair.Value switch
            {
                string strValue => strValue.Trim(),
                _ => pair.Value
            });
    }

    private static string SanitizeIp(string ipAddress)
    {
        var trimmed = ipAddress.Trim();
        if (!IpRegex().IsMatch(trimmed))
        {
            return trimmed;
        }

        return trimmed;
    }

    [GeneratedRegex(@"^(?:\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled)]
    private static partial Regex IpRegex();
}


