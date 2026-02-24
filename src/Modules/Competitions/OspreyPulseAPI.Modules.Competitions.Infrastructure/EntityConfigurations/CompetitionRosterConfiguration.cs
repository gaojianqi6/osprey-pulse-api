using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class CompetitionRosterConfiguration : IEntityTypeConfiguration<CompetitionRoster>
{
    public void Configure(EntityTypeBuilder<CompetitionRoster> builder)
    {
        builder.ToTable("competition_rosters");

        builder.Property(e => e.PositionPlayed).HasColumnName("position_played");
        builder.Property(e => e.MinutesPlayed).HasColumnName("minutes_played");
        builder.Property(e => e.Stats)
            .HasColumnName("stats")
            .HasColumnType("jsonb");
    }
}
