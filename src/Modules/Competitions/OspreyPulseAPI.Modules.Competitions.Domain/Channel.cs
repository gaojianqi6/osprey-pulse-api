namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Top-level sport or category (e.g. NBA, Rugby, Gaming).
/// Maps to: channels
/// </summary>
public class Channel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    /// <summary>Unique slug: 'nba', 'rugby', 'gaming'</summary>
    public required string Slug { get; set; }
    /// <summary>Sport type: 'basketball', 'rugby', 'esports'</summary>
    public string? SportType { get; set; }
    public string? Description { get; set; }

    public ICollection<League> Leagues { get; set; } = new List<League>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
