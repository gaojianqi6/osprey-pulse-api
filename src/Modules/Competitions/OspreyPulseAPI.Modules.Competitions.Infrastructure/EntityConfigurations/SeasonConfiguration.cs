using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("Seasons");
        builder.Property(e => e.YearLabel).HasColumnName("year_label");
        builder.HasIndex(e => new { e.LeagueId, e.YearLabel }).IsUnique();
    }
}
