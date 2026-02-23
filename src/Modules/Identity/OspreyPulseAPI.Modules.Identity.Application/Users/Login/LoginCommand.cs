using MediatR;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.Login;

public record LoginCommand(string LoginName, string Password) : IRequest<LoginResponse>;

public record LoginResponse(string AccessToken, string Username, long OspreyPoints);
