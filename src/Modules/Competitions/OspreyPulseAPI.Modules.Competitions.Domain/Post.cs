namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Post or news item. Maps to: posts.
/// user_id nullable (null for news), channel_id required.
/// origin_data stores raw payload (e.g. ESPN article JSON).
/// </summary>
public class Post
{
    public long Id { get; set; }
    /// <summary>Null for news; set for user posts.</summary>
    public Guid? UserId { get; set; }
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public required string Title { get; set; }
    public string? Content { get; set; }
    public string? ShortDescription { get; set; }
    public string? PreviewImg { get; set; }
    /// <summary>Raw JSON (e.g. ESPN article). Stored as JSONB.</summary>
    public string? OriginData { get; set; }
    /// <summary>External source id (e.g. ESPN article id) for deduplication.</summary>
    public string? ExternalId { get; set; }
    public PostType Type { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastBumpedAt { get; set; }
}
