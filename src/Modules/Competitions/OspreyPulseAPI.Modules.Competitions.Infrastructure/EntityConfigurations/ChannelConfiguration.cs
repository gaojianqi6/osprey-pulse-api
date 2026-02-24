using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");
    }
}
