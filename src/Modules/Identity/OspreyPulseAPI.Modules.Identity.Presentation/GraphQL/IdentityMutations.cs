using MediatR;
using HotChocolate.Types;
using OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

namespace OspreyPulseAPI.Modules.Identity.Presentation.GraphQL;

[ExtendObjectType("Mutation")]
public class IdentityMutations
{
    public async Task<Guid> RegisterUserAsync(
        string username,
        string email,
        Guid supabaseId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(supabaseId, username, email);
        return await mediator.Send(command, cancellationToken);
    }
}
