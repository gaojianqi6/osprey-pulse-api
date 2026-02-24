using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("competitions");

        builder.HasIndex(e => e.ExternalId).IsUnique().HasFilter("\"ExternalId\" IS NOT NULL");
        builder.HasIndex(e => new { e.SeasonId, e.HomeTeamId, e.AwayTeamId, e.StartTime }).IsUnique();

        builder.HasOne(c => c.HomeTeam)
            .WithMany(t => t.HomeCompetitions)
            .HasForeignKey(c => c.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AwayTeam)
            .WithMany(t => t.AwayCompetitions)
            .HasForeignKey(c => c.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");
    }
}
