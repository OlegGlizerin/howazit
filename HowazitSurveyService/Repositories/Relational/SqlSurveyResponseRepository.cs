using System.Text.Json;
using HowazitSurveyService.Model;
using HowazitSurveyService.Model.Entities;
using HowazitSurveyService.Model.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HowazitSurveyService.Repositories.Relational;

public sealed class SqlSurveyResponseRepository : IRelationalSurveyResponseRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SqlSurveyResponseRepository> _logger;

    public SqlSurveyResponseRepository(ApplicationDbContext dbContext, ILogger<SqlSurveyResponseRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task UpsertAsync(SurveyResponse response, string encryptedIpAddress, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.SurveyResponses
            .FirstOrDefaultAsync(x => x.ClientId == response.ClientId && x.ResponseId == response.ResponseId, cancellationToken);

        if (existing is null)
        {
            var entity = MapToEntity(response, encryptedIpAddress);
            await _dbContext.SurveyResponses.AddAsync(entity, cancellationToken);
        }
        else
        {
            existing.SurveyId = response.SurveyId;
            existing.NpsScore = response.NpsScore;
            existing.Satisfaction = response.Satisfaction;
            existing.CustomFieldsJson = SerializeCustomFields(response.CustomFields);
            existing.SubmittedAtUtc = response.SubmittedAtUtc;
            existing.UserAgent = response.UserAgent;
            existing.EncryptedIpAddress = encryptedIpAddress;
            existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to upsert survey response for client {ClientId} response {ResponseId}", response.ClientId, response.ResponseId);
            throw;
        }
    }

    private static SurveyResponseEntity MapToEntity(SurveyResponse response, string encryptedIpAddress)
    {
        var now = DateTimeOffset.UtcNow;
        return new SurveyResponseEntity
        {
            SurveyId = response.SurveyId,
            ClientId = response.ClientId,
            ResponseId = response.ResponseId,
            NpsScore = response.NpsScore,
            Satisfaction = response.Satisfaction,
            CustomFieldsJson = SerializeCustomFields(response.CustomFields),
            SubmittedAtUtc = response.SubmittedAtUtc,
            UserAgent = response.UserAgent,
            EncryptedIpAddress = encryptedIpAddress,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    private static string? SerializeCustomFields(IDictionary<string, object?> customFields)
    {
        if (customFields.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(customFields);
    }
}

