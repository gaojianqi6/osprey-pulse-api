using MediatR;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Domain;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly ISupabaseAuthService _supabase;
    private readonly IIdentityDbContext _context;

    public RegisterUserHandler(ISupabaseAuthService supabase, IIdentityDbContext context)
    {
        _supabase = supabase;
        _context = context;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var options = new Dictionary<string, string> { { "username", request.Username } };
        var userId = await _supabase.SignUpAsync(request.Email, request.Password, options, ct);

        var newUser = new User
        {
            Id = userId,
            Username = request.Username,
            Email = request.Email,
            OspreyPoints = 1000,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(ct);

        return newUser.Id;
    }
}
