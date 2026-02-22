using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class PlayerTeamAssignmentConfiguration : IEntityTypeConfiguration<PlayerTeamAssignment>
{
    public void Configure(EntityTypeBuilder<PlayerTeamAssignment> builder)
    {
        builder.ToTable("player_team_assignments");

        // Composite primary key (EF Core needs this explicit for join tables)
        builder.HasKey(pta => new { pta.PlayerId, pta.TeamId });

        // Explicit foreign keys and relationships so EF Core maps correctly
        builder.HasOne(pta => pta.Player)
            .WithMany(p => p.TeamAssignments)
            .HasForeignKey(pta => pta.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pta => pta.Team)
            .WithMany(t => t.PlayerAssignments)
            .HasForeignKey(pta => pta.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
