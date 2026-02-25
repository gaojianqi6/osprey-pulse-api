using HotChocolate.Execution.Configuration;
using OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

namespace OspreyPulseAPI.Api.GraphQL;

public static class CompetitionsModuleExtensions
{
    public static IRequestExecutorBuilder AddCompetitionsModule(this IRequestExecutorBuilder builder)
    {
        return builder
            .AddTypeExtension<CompetitionsQueries>()
            .AddTypeExtension<CompetitionRosterExtensions>();
    }
}

