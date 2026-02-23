using HotChocolate.Execution.Configuration;
using OspreyPulseAPI.Modules.Identity.Presentation.GraphQL;

namespace OspreyPulseAPI.Api.GraphQL;

public static class IdentityModuleExtensions
{
    public static IRequestExecutorBuilder AddIdentityModule(this IRequestExecutorBuilder builder)
    {
        return builder.AddTypeExtension<IdentityMutations>();
    }
}
