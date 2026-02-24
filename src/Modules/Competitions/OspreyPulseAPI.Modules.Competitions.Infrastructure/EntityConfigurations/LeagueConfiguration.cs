using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");
        builder.HasIndex(e => e.ExternalId).IsUnique().HasFilter("\"ExternalId\" IS NOT NULL");
    }
}
