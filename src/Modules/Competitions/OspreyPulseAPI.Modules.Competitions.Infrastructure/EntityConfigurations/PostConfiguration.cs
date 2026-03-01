using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.EntityConfigurations;

internal class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.Property(p => p.Title).HasMaxLength(255);

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.OriginData)
            .HasColumnName("origin_data")
            .HasColumnType("jsonb");

        builder.Property(p => p.ShortDescription)
            .HasColumnName("short_description");

        builder.Property(p => p.PreviewImg)
            .HasColumnName("preview_img");

        builder.Property(p => p.ExternalId)
            .HasColumnName("external_id");

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(p => p.LastBumpedAt)
            .HasColumnName("last_bumped_at");

        builder.HasIndex(p => new { p.ChannelId, p.Type, p.CreatedAt });
        builder.HasIndex(p => new { p.ChannelId, p.ExternalId })
            .IsUnique()
            .HasFilter("\"external_id\" IS NOT NULL");

        builder.HasOne(p => p.Channel)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
