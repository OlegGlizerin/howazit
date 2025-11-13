using HowazitSurveyService.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HowazitSurveyService.Model;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<SurveyResponseEntity> SurveyResponses => Set<SurveyResponseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SurveyResponseEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.ClientId, e.ResponseId })
                  .IsUnique();

            entity.Property(e => e.SurveyId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.ClientId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.ResponseId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Satisfaction)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.CustomFieldsJson);

            entity.Property(e => e.UserAgent)
                  .HasMaxLength(512);

            entity.Property(e => e.EncryptedIpAddress)
                  .HasMaxLength(512);

            entity.Property(e => e.SubmittedAtUtc)
                  .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                  .IsRequired();

            entity.Property(e => e.UpdatedAtUtc)
                  .IsRequired();
        });
    }
}

