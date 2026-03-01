using HotChocolate.Types;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

public class PostTypeExtensions : ObjectTypeExtension<Post>
{
    protected override void Configure(IObjectTypeDescriptor<Post> descriptor)
    {
        descriptor.Ignore(p => p.OriginData);
    }
}
