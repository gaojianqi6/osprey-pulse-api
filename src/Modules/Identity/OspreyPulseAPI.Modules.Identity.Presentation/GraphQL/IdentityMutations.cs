using MediatR;
using HotChocolate.Types;
using OspreyPulseAPI.Modules.Identity.Application.Users.Login;
using OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

namespace OspreyPulseAPI.Modules.Identity.Presentation.GraphQL;

[ExtendObjectType("Mutation")]
public class IdentityMutations
{
    public async Task<Guid> RegisterUser(
        string username,
        string email,
        string password,
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
        => await mediator.Send(new RegisterUserCommand(username, email, password), cancellationToken);

    public async Task<LoginResponse> Login(
        string loginName,
        string password,
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
        => await mediator.Send(new LoginCommand(loginName, password), cancellationToken);
}
