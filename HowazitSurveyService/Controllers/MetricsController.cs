using HowazitSurveyService.Services.Insights;
using HowazitSurveyService.Services.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace HowazitSurveyService.Controllers;

[ApiController]
[Route("api/metrics")]
public sealed class MetricsController(
    IMetricsService metricsService,
    IInsightsCacheService insightsCache,
    ILogger<MetricsController> logger) : ControllerBase
{
    [HttpGet("nps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetNpsByClient()
    {
        try
        {
            var metrics = metricsService.GetClientNpsMetrics();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NPS metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to retrieve NPS metrics at this time"
            });
        }
    }

    [HttpGet("insights")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetInsights()
    {
        try
        {
            var insights = insightsCache.GetLatest();
            return Ok(insights);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve insights");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to retrieve insights at this time"
            });
        }
    }
}

