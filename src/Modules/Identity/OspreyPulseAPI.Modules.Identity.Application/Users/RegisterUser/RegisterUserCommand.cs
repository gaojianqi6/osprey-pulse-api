using MediatR;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

/// <summary>
/// Command to register a new user via Supabase Auth and create the profile in identity.Users.
/// </summary>
public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<Guid>;
