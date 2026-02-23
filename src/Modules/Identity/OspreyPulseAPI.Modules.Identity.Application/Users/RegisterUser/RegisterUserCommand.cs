using MediatR;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

/// <summary>
/// Command to register a new user (e.g. after Supabase Auth signup).
/// </summary>
public record RegisterUserCommand(
    Guid Id,
    string Username,
    string Email
) : IRequest<Guid>;
