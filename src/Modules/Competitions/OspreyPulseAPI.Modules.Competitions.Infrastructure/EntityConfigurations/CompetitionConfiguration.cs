using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("competitions");

        // Two FKs to Team: EF Core needs explicit configuration to match navigations to FKs
        builder.HasOne(c => c.HomeTeam)
            .WithMany(t => t.HomeCompetitions)
            .HasForeignKey(c => c.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AwayTeam)
            .WithMany(t => t.AwayCompetitions)
            .HasForeignKey(c => c.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL JSONB: ensures searchable/indexable object, not plain text
        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");
    }
}
