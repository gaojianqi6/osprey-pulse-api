using MediatR;
using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ISupabaseAuthService _supabase;
    private readonly IIdentityDbContext _context;

    public LoginHandler(ISupabaseAuthService supabase, IIdentityDbContext context)
    {
        _supabase = supabase;
        _context = context;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.LoginName;

        if (!request.LoginName.Contains('@'))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.LoginName, ct);
            if (user == null)
                throw new InvalidOperationException("User not found.");
            email = user.Email;
        }

        var accessToken = await _supabase.SignInAsync(email, request.Password, ct);

        var profile = await _context.Users.FirstAsync(u => u.Email == email, ct);

        return new LoginResponse(accessToken, profile.Username, profile.OspreyPoints);
    }
}
