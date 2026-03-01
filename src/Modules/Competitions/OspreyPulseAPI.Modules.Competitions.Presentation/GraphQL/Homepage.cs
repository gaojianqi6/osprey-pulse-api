using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

/// <summary>
/// Homepage payload: today's NBA competitions and latest posts (news).
/// </summary>
public class Homepage
{
    public List<Competition> NbaTodayCompetitions { get; set; } = new();
    public List<Post> NbaPosts { get; set; } = new();
}
