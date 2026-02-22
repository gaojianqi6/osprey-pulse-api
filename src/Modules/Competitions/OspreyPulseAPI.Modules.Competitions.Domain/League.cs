namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// League within a channel (e.g. NBA, NZNBL).
/// Maps to: leagues
/// </summary>
public class League
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public required string Name { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
