using FluentValidation;
using HowazitSurveyService.Model.Dtos;
using HowazitSurveyService.Services.Submission;
using Microsoft.AspNetCore.Mvc;

namespace HowazitSurveyService.Controllers;

[ApiController]
[Route("api/surveys/responses")]
public sealed class SurveyResponsesController(
    ISurveyResponseSubmissionService submissionService,
    ILogger<SurveyResponsesController> logger)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IngestAsync([FromBody] SurveyResponseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await submissionService.SubmitAsync(request, cancellationToken);

            return Accepted(new
            {
                message = "Survey response queued for processing",
                result.ResponseId,
                result.ClientId
            });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue survey response");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to process survey response at this time"
            });
        }
    }
}

