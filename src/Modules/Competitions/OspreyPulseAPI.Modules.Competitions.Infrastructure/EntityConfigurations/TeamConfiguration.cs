using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasIndex(e => e.ExternalId).IsUnique().HasFilter("\"ExternalId\" IS NOT NULL");
    }
}
