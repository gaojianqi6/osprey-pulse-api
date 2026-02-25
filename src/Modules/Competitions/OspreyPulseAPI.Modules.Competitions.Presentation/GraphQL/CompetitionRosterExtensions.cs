using HotChocolate;
using HotChocolate.Types;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

[ExtendObjectType(typeof(CompetitionRoster))]
public class CompetitionRosterExtensions
{
    /// <summary>
    /// Jersey number for the player on this team (from PlayerTeamAssignment).
    /// </summary>
    [GraphQLName("jerseyNumber")]
    public short? GetJerseyNumber([Parent] CompetitionRoster roster)
    {
        if (roster.Player == null)
        {
            return null;
        }

        var assignment = roster.Player.TeamAssignments
            .FirstOrDefault(a => a.TeamId == roster.TeamId && a.IsActive);

        return assignment?.JerseyNumber;
    }
}
