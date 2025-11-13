using HowazitSurveyService.Model.Dtos;
using HowazitSurveyService.Services.Sanitization;

namespace HowazitSurveyService.Tests.Services.Sanitization;

public sealed class SurveyResponseSanitizerTests
{
    [Fact]
    public void Sanitize_TrimsStringsAndCustomFields()
    {
        // Arrange
        var sanitizer = new SurveyResponseSanitizerService();

        var request = new SurveyResponseRequest
        {
            SurveyId = " survey ",
            ClientId = " client ",
            ResponseId = " response ",
            Responses = new SurveyResponsesBody
            {
                NpsScore = 9,
                Satisfaction = " happy ",
                CustomFields = new Dictionary<string, object?>
                {
                    [" name "] = " value ",
                    ["count"] = 5
                }
            },
            Metadata = new SurveyResponseMetadata
            {
                Timestamp = DateTimeOffset.UtcNow,
                UserAgent = " agent ",
                IpAddress = " 127.0.0.1 "
            }
        };

        // Act
        var sanitized = sanitizer.Sanitize(request);

        // Assert
        Assert.Equal("survey", sanitized.SurveyId);
        Assert.Equal("client", sanitized.ClientId);
        Assert.Equal("response", sanitized.ResponseId);
        Assert.Equal("happy", sanitized.Responses.Satisfaction);
        Assert.Equal("agent", sanitized.Metadata.UserAgent);
        Assert.Equal("127.0.0.1", sanitized.Metadata.IpAddress);
        Assert.Contains("name", sanitized.Responses.CustomFields!.Keys);
        Assert.Equal("value", sanitized.Responses.CustomFields!["name"]);
        Assert.Equal(5, sanitized.Responses.CustomFields!["count"]);
    }
}

