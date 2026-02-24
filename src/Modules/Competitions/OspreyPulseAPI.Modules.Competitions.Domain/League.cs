namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// League within a channel (e.g. NBA Standard, Africa). ExternalId = API-Sports league key.
/// Maps to: leagues
/// </summary>
public class League
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    /// <summary>API-Sports league id (e.g. 'standard', 'africa'). Unique.</summary>
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    /// <summary>Short code: 'NBA', 'NBL'</summary>
    public string? ShortCode { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
